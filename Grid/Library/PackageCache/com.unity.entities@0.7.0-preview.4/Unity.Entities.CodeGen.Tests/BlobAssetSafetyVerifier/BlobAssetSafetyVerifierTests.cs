﻿using System;
using System.Collections.Generic;
using Mono.Cecil;
using NUnit.Framework;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.Entities.CodeGen.Tests.TestTypes;

namespace Unity.Entities.CodeGen.Tests
{
    [TestFixture]
    public class BlobAssetSafetyVerifierTests : PostProcessorTestBase
    {
        public struct MyBlob
        {
            public BlobArray<float> myfloats;
        }

        class StoreBlobAssetReferenceValueInLocal_Class
        {
            static BlobAssetReference<MyBlob> _blobAssetReference;
            
            void Method()
            {
                MyBlob blob = _blobAssetReference.Value;
                EnsureNotOptimizedAway(blob.myfloats.Length);
            }
        }

        [Test]
        public void StoreBlobAssetReferenceValueInLocal()
        {
            AssertProducesError(
                typeof(StoreBlobAssetReferenceValueInLocal_Class), 
                "error MayOnlyLiveInBlobStorageViolation: MyBlob may only live in blob storage. Access it by ref instead: `ref MyBlob yourVariable = ref ...`");
        }

        class LoadFieldFromBlobAssetReference_Class
        {
            static BlobAssetReference<MyBlob> _blobAssetReference;
            
            void Method()
            {
                BlobArray<float> myFloats = _blobAssetReference.Value.myfloats;
                EnsureNotOptimizedAway(myFloats.Length);
            }
        }

        [Test]
        public void LoadFieldFromBlobAssetReference()
        {
            AssertProducesError(
                typeof(LoadFieldFromBlobAssetReference_Class), 
                " error MayOnlyLiveInBlobStorageViolation: You may only access .myfloats by ref, as it may only live in blob storage. try `ref BlobArray<Single> yourVariable = ref yourMyBlob.myfloats`");
        }

        class WithReferenceToValidType_Class
        {
            BoidInAnotherAssembly someField;
            void Method()
            {
                this.someField = new BoidInAnotherAssembly();
                EnsureNotOptimizedAway(this.someField);
            }
        }
        
        [Test]
        public void FailResolveWithWarning()
        {
            AssertProducesWarning(typeof(WithReferenceToValidType_Class), 
                "ResolveFailureWarning: Unable to resolve type Unity.Entities.CodeGen.Tests.TestTypes.BoidInAnotherAssembly for verification", true);
        }
        
        class ClassWithValidBlobReferenceUsage
        {
            public class GenericTypeWithVolatile<T>
            {
                public volatile T[] buffer;
                public T this[int i] { get { return buffer[i]; } set { buffer[i] = value; } }
            }
            GenericTypeWithVolatile<int> _intGeneric;
            BoidInAnotherAssembly _someField;
            BlobAssetReference<MyBlob> _blobAssetReference;
            
            void Method()
            {
                _intGeneric = new GenericTypeWithVolatile<int>();
                _intGeneric.buffer = new[] {32, 12, 41};
                EnsureNotOptimizedAway(_intGeneric.buffer);
                
                _someField = new BoidInAnotherAssembly();
                EnsureNotOptimizedAway(_someField);
                
                ref BlobArray<float> myFloats = ref _blobAssetReference.Value.myfloats;
                EnsureNotOptimizedAway(myFloats.Length);
                
                ref MyBlob blob = ref _blobAssetReference.Value;
                EnsureNotOptimizedAway(blob.myfloats.Length);
            }
        }
        
        [Test]
        public void ValidBlobReferenceUsageSucceeds()
        {
            AssertProducesNoError(typeof(ClassWithValidBlobReferenceUsage));
        }

        protected void AssertProducesNoError(Type typeWithCodeUnderTest)
        {
            Assert.DoesNotThrow(() =>
            {
                var methodToAnalyze = MethodDefinitionForOnlyMethodOf(typeWithCodeUnderTest);
                var diagnosticMessages = new List<DiagnosticMessage>();
                
                try
                {
                    var verifyDiagnosticMessages = BlobAssetSafetyVerifier.VerifyMethod(methodToAnalyze, new HashSet<TypeReference>());
                    diagnosticMessages.AddRange(verifyDiagnosticMessages);
                }
                catch (FoundErrorInUserCodeException exc)
                {
                    diagnosticMessages.AddRange(exc.DiagnosticMessages);
                }
                
                Assert.AreEqual(0, diagnosticMessages.Count);
            });
        }

        void AssertProducesInternal(Type typeWithCodeUnderTest, DiagnosticType diagnosticType, string shouldContain, bool failResolve = false)
        {
            var methodToAnalyze = MethodDefinitionForOnlyMethodOf(typeWithCodeUnderTest, failResolve);
            var diagnosticMessages = new List<DiagnosticMessage>();

            try
            {
                var verifyDiagnosticMessages = BlobAssetSafetyVerifier.VerifyMethod(methodToAnalyze, new HashSet<TypeReference>());
                diagnosticMessages.AddRange(verifyDiagnosticMessages);
            }
            catch (FoundErrorInUserCodeException exc)
            {
                diagnosticMessages.AddRange(exc.DiagnosticMessages);
            }

            Assert.AreEqual(1, diagnosticMessages.Count);
            Assert.AreEqual(diagnosticType, diagnosticMessages[0].DiagnosticType);

            foreach(var diagnosticMessage in diagnosticMessages)
                StringAssert.Contains(shouldContain, diagnosticMessage.MessageData);
            
            AssertDiagnosticHasSufficientFileAndLineInfo(diagnosticMessages);
        }
        
        protected void AssertProducesWarning(Type typeWithCodeUnderTest, string shouldContain, bool failResolve = false)
        {
            AssertProducesInternal(typeWithCodeUnderTest, DiagnosticType.Warning, shouldContain, failResolve);
        }

        protected void AssertProducesError(Type typeWithCodeUnderTest, string shouldContain, bool failResolve = false)
        {
            AssertProducesInternal(typeWithCodeUnderTest, DiagnosticType.Error, shouldContain, failResolve);
        }
    }
}