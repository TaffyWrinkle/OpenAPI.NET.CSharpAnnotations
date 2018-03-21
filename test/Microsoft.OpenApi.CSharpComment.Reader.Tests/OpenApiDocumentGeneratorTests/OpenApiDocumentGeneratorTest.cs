﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.OpenApi.CSharpComment.Reader.DocumentFilters;
using Microsoft.OpenApi.CSharpComment.Reader.Exceptions;
using Microsoft.OpenApi.CSharpComment.Reader.Extensions;
using Microsoft.OpenApi.CSharpComment.Reader.Models;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.OpenApi.CSharpComment.Reader.Tests.OpenApiDocumentGeneratorTests
{
    [Collection("DefaultSettings")]
    public class OpenApiDocumentGeneratorTest
    {
        private const string InputDirectory = "OpenApiDocumentGeneratorTests/Input";
        private const string OutputDirectory = "OpenApiDocumentGeneratorTests/Output";

        private readonly ITestOutputHelper _output;

        public OpenApiDocumentGeneratorTest(ITestOutputHelper output)
        {
            _output = output;
        }

        public static IEnumerable<object[]> GetTestCasesForInvalidDocumentationShouldYieldFailure()
        {
            // Invalid Verb
            yield return new object[]
            {
                "Invalid Verb",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationInvalidVerb.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationInvalidVerb.Json"),
                new DocumentGenerationDiagnostic
                {
                    Errors =
                    {
                        new GenerationError
                        {
                            ExceptionType = typeof(UnableToGenerateAllOperationsException).Name,
                            Message = string.Format(
                                SpecificationGenerationMessages.UnableToGenerateAllOperations,
                                8,
                                9),
                        }
                    }
                },
                new List<OperationGenerationDiagnostic>
                {
                    new OperationGenerationDiagnostic
                    {
                        OperationMethod = "Invalid",
                        Path = "/V1/samples/{id}",
                        Errors =
                        {
                            new GenerationError
                            {
                                ExceptionType = typeof(InvalidVerbException).Name,
                                Message = string.Format(SpecificationGenerationMessages.InvalidHttpMethod, "Invalid"),
                            }
                        }
                    }
                }
            };

            // Invalid Uri
            yield return new object[]
            {
                "Invalid Uri",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationInvalidUri.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationInvalidUri.Json"),
                new DocumentGenerationDiagnostic
                {
                    Errors =
                    {
                        new GenerationError
                        {
                            ExceptionType = typeof(UnableToGenerateAllOperationsException).Name,
                            Message = string.Format(
                                SpecificationGenerationMessages.UnableToGenerateAllOperations,
                                8,
                                9),
                        }
                    }
                },
                new List<OperationGenerationDiagnostic>
                {
                    new OperationGenerationDiagnostic
                    {
                        OperationMethod = SpecificationGenerationMessages.OperationMethodNotParsedGivenUrlIsInvalid,
                        Path = "http://{host}:9000/V1/samples/{id}?queryBool={queryBool}",
                        Errors =
                        {
                            new GenerationError
                            {
                                ExceptionType = typeof(InvalidUrlException).Name,
                                Message = string.Format(
                                    SpecificationGenerationMessages.InvalidUrl,
                                    "http://{host}:9000/V1/samples/{id}?queryBool={queryBool}",
                                    SpecificationGenerationMessages.MalformattedUrl),
                            }
                        }
                    }
                }
            };
        }

        public static IEnumerable<object[]> GetTestCasesForInvalidDocumentationShouldRemoveFailedOperations()
        {
            // Parameters that have no in attributes and not present in the URL.
            yield return new object[]
            {
                "Parameters Without In Attribute And Not Present In URL",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationParamWithoutInNotPresentInUrl.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationParamWithoutInNotPresentInUrl.Json"),
                new DocumentGenerationDiagnostic
                {
                    Errors =
                    {
                        new GenerationError
                        {
                            ExceptionType = typeof(UnableToGenerateAllOperationsException).Name,
                            Message = string.Format(
                                SpecificationGenerationMessages.UnableToGenerateAllOperations,
                                8,
                                9),
                        }
                    }
                },
                new List<OperationGenerationDiagnostic>
                {
                    new OperationGenerationDiagnostic
                    {
                        OperationMethod = OperationType.Get.ToString(),
                        Path = "/V1/samples/{id}",
                        Errors =
                        {
                            new GenerationError
                            {
                                ExceptionType = typeof(MissingInAttributeException).Name,
                                Message = string.Format(
                                    SpecificationGenerationMessages.MissingInAttribute,
                                    string.Join(", ", new List<string> {"sampleHeaderParam2", "sampleHeaderParam3"})),
                            }
                        }
                    }
                }
            };

            // Conflicting Path and Query Parameters
            yield return new object[]
            {
                "Conflicting Path and Query Parameters",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationConflictingPathAndQueryParameters.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationConflictingPathAndQueryParameters.Json"),
                new DocumentGenerationDiagnostic
                {
                    Errors =
                    {
                        new GenerationError
                        {
                            ExceptionType = typeof(UnableToGenerateAllOperationsException).Name,
                            Message = string.Format(
                                SpecificationGenerationMessages.UnableToGenerateAllOperations,
                                8,
                                9),
                        }
                    }
                },
                new List<OperationGenerationDiagnostic>
                {
                    new OperationGenerationDiagnostic
                    {
                        OperationMethod = OperationType.Get.ToString(),
                        Path = "/V1/samples/{id}",
                        Errors =
                        {
                            new GenerationError
                            {
                                ExceptionType = typeof(ConflictingPathAndQueryParametersException).Name,
                                Message = string.Format(
                                    SpecificationGenerationMessages.ConflictingPathAndQueryParameters,
                                    "id",
                                    "http://localhost:9000/V1/samples/{id}?queryBool={queryBool}&id={id}"),
                            }
                        }
                    }
                }
            };

            // Path parameter in the URL is not documented in any param elements.
            yield return new object[]
            {
                "Path Parameter Undocumented",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationUndocumentedPathParam.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationUndocumentedPathParam.Json"),
                new DocumentGenerationDiagnostic
                {
                    Errors =
                    {
                        new GenerationError
                        {
                            ExceptionType = typeof(UnableToGenerateAllOperationsException).Name,
                            Message = string.Format(
                                SpecificationGenerationMessages.UnableToGenerateAllOperations,
                                8,
                                9),
                        }
                    }
                },
                new List<OperationGenerationDiagnostic>
                {
                    new OperationGenerationDiagnostic
                    {
                        OperationMethod = OperationType.Get.ToString(),
                        Path = "/V1/samples/{id}",
                        Errors =
                        {
                            new GenerationError
                            {
                                ExceptionType = typeof(UndocumentedPathParameterException).Name,
                                Message = string.Format(
                                    SpecificationGenerationMessages.UndocumentedPathParameter,
                                    "id",
                                    "http://localhost:9000/V1/samples/{id}?queryBool={queryBool}"),
                            }
                        }
                    }
                }
            };

            // Undocumented Generics
            yield return new object[]
            {
                "Undocumented Generics",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationUndocumentedGeneric.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationUndocumentedGeneric.Json"),
                new DocumentGenerationDiagnostic
                {
                    Errors =
                    {
                        new GenerationError
                        {
                            ExceptionType = typeof(UnableToGenerateAllOperationsException).Name,
                            Message = string.Format(
                                SpecificationGenerationMessages.UnableToGenerateAllOperations,
                                8,
                                9),
                        }
                    }
                },
                new List<OperationGenerationDiagnostic>
                {
                    new OperationGenerationDiagnostic
                    {
                        OperationMethod = OperationType.Get.ToString(),
                        Path = "/V3/samples/{id}",
                        Errors =
                        {
                            new GenerationError
                            {
                                ExceptionType = typeof(UndocumentedGenericTypeException).Name,
                                Message = SpecificationGenerationMessages.UndocumentedGenericType,
                            }
                        }
                    }
                }
            };

            // Incorrect Order for Generics
            yield return new object[]
            {
                "Incorrect Order for Generics",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationIncorrectlyOrderedGeneric.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationIncorrectlyOrderedGeneric.Json"),
                new DocumentGenerationDiagnostic
                {
                    Errors =
                    {
                        new GenerationError
                        {
                            ExceptionType = typeof(UnableToGenerateAllOperationsException).Name,
                            Message = string.Format(
                                SpecificationGenerationMessages.UnableToGenerateAllOperations,
                                8,
                                9),
                        }
                    }
                },
                new List<OperationGenerationDiagnostic>
                {
                    new OperationGenerationDiagnostic
                    {
                        OperationMethod = OperationType.Get.ToString(),
                        Path = "/V3/samples/",
                        Errors =
                        {
                            new GenerationError
                            {
                                ExceptionType = typeof(UnorderedGenericTypeException).Name,
                                Message = SpecificationGenerationMessages.UnorderedGenericType,
                            }
                        }
                    }
                }
            };

            // Body parameter missing see tag
            yield return new object[]
            {
                "Body parameter missing see tag",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationRequestMissingSeeTag.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationRequestMissingSeeTag.Json"),
                new DocumentGenerationDiagnostic
                {
                    Errors =
                    {
                        new GenerationError
                        {
                            ExceptionType = typeof(UnableToGenerateAllOperationsException).Name,
                            Message = string.Format(
                                SpecificationGenerationMessages.UnableToGenerateAllOperations,
                                8,
                                9),
                        }
                    }
                },
                new List<OperationGenerationDiagnostic>
                {
                    new OperationGenerationDiagnostic
                    {
                        OperationMethod = OperationType.Post.ToString(),
                        Path = "/V3/samples/{id}",
                        Errors =
                        {
                            new GenerationError
                            {
                                ExceptionType = typeof(InvalidRequestBodyException).Name,
                                Message = string.Format(
                                    SpecificationGenerationMessages.MissingSeeCrefTag,
                                    "sampleObject"),
                            }
                        }
                    }
                }
            };

            // Type not found in provided contract assemblies
            yield return new object[]
            {
                "Type not found",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationTypeNotFound.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationTypeNotFound.Json"),
                new DocumentGenerationDiagnostic
                {
                    Errors =
                    {
                        new GenerationError
                        {
                            ExceptionType = typeof(UnableToGenerateAllOperationsException).Name,
                            Message = string.Format(
                                SpecificationGenerationMessages.UnableToGenerateAllOperations,
                                8,
                                9),
                        }
                    }
                },
                new List<OperationGenerationDiagnostic>
                {
                    new OperationGenerationDiagnostic
                    {
                        OperationMethod = OperationType.Post.ToString(),
                        Path = "/V3/samples/{id}",
                        Errors =
                        {
                            new GenerationError
                            {
                                ExceptionType = typeof(TypeLoadException).Name,
                                Message = string.Format(
                                    SpecificationGenerationMessages.TypeNotFound,
                                    "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.TestNotFound",
                                string.Join(" ", new List<string>
                                {
                                    "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll",
                                    "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll"
                                }))
                            }
                        }
                    }
                }
            };

            // The response is missing description
            yield return new object[]
            {
                "Response missing description",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationResponseMissingDescription.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationResponseMissingDescription.Json"),
                new DocumentGenerationDiagnostic
                {
                    Errors =
                    {
                        new GenerationError
                        {
                            ExceptionType = typeof(UnableToGenerateAllOperationsException).Name,
                            Message = string.Format(
                                SpecificationGenerationMessages.UnableToGenerateAllOperations,
                                8,
                                9),
                        }
                    }
                },
                new List<OperationGenerationDiagnostic>
                {
                    new OperationGenerationDiagnostic
                    {
                        OperationMethod = OperationType.Get.ToString(),
                        Path = "/V3/samples/{id}",
                        Errors =
                        {
                            new GenerationError
                            {
                                ExceptionType = typeof(MissingResponseDescriptionException).Name,
                                Message = string.Format(
                                    SpecificationGenerationMessages.MissingResponseDescription,
                                    "400")
                            }
                        }
                    }
                }
            };
        }

        public static IEnumerable<object[]> GetTestCasesForPassANewFilterAndShouldReturnCorrectDocument()
        {
            // Standard, original valid XML document
            yield return new object[]
            {
                "Standard valid XML document",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationNewFilter.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                new UpdateSchemaWithNewtonsoftJsonPropertyAttributeFilter(),
                OpenApiSpecVersion.OpenApi3_0,
                1,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationNewFilter.Json")
            };
        }

        public static IEnumerable<object[]> GetTestCasesForValidDocumentationShouldReturnCorrectDocument()
        {
            // Standard, original valid XML document
            yield return new object[]
            {
                "Standard valid XML document",
                new List<string>
                {
                    Path.Combine(InputDirectory, "Annotation.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "Annotation.Json")
            };

            // Standard, original XML document with no response body
            yield return new object[]
            {
                "Standard valid XML document with no response body.",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationWithNoResponseBody.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                1,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationWithNoResponseBody.Json")
            };

            // Valid XML document but with parameters that have no in attributes but are present in the URL.
            yield return new object[]
            {
                "Parameters Without In Attribute But Present In URL",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationParamWithoutInButPresentInUrl.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationParamWithoutInButPresentInUrl.Json")
            };

            // Valid XML document but with one parameter without specified type.
            // The type should simply default to string.
            yield return new object[]
            {
                "Unspecified Type Default to String",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationParamNoTypeSpecified.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationParamNoTypeSpecified.Json")
            };

            // Valid XML document with multiple response types per response code.
            yield return new object[]
            {
                "Multiple Response Types Per Response Code",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationMultipleResponseTypes.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationMultipleResponseTypes.Json")
            };

            // Valid XML document with multiple request types.
            yield return new object[]
            {
                "Multiple Request Types",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationMultipleRequestTypes.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationMultipleRequestTypes.Json")
            };

            // Valid XML document with multiple request content types.
            yield return new object[]
            {
                "Multiple Request Media Types",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationMultipleRequestMediaTypes.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationMultipleRequestMediaTypes.Json")
            };

            // Valid XML document with multiple response content types.
            yield return new object[]
            {
                "Multiple Response Media Types Per Response Code",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationMultipleResponseMediaTypes.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationMultipleResponseMediaTypes.Json")
            };

            // Valid XML document with optional path parameters.
            yield return new object[]
            {
                "Optional Path Parameters",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationOptionalPathParametersBranching.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationOptionalPathParametersBranching.Json")
            };

            // Valid XML document with alternative param tags.
            yield return new object[]
            {
                "Alternative Param Tags (i.e. queryParam, pathParam, header)",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationAlternativeParamTags.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationAlternativeParamTags.Json")
            };

            // Valid XML document with array type in param tags.
            yield return new object[]
            {
                "Array Type in Param Tags",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationArrayInParamTags.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationArrayInParamTags.Json")
            };

            // Valid XML document with summary including tags
            yield return new object[]
            {
                "Summary With Tags (see cref or paramref)",
                new List<string>
                {
                    Path.Combine(InputDirectory, "AnnotationSummaryWithTags.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationSummaryWithTags.Json")
            };
        }

        /// <summary>
        /// A short version of the <see cref="GetTestCasesForValidDocumentationShouldReturnCorrectDocument"/>
        /// so that we can simply test the serialization without wasting time on all test cases.
        /// </summary> 
        public static IEnumerable<object[]> GetTestCasesForValidDocumentationShouldReturnCorrectSerializedDocument()
        {
            // Standard, original valid XML document with JSON - V3 Open Api Document as output
            yield return new object[]
            {
                "Standard valid XML document (V3-JSON)",
                new List<string>
                {
                    Path.Combine(InputDirectory, "Annotation.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                OpenApiFormat.Json,
                9,
                Path.Combine(
                    OutputDirectory,
                    "Annotation.Json")
            };

            // Standard, original valid XML document with YAML - V3 Open Api Document as output
            yield return new object[]
            {
                "Standard valid XML document (V3-YAML)",
                new List<string>
                {
                    Path.Combine(InputDirectory, "Annotation.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi3_0,
                OpenApiFormat.Yaml,
                9,
                Path.Combine(
                    OutputDirectory,
                    "Annotation.Json")
            };

            // Standard, original valid XML document with YAML - V2 Open Api Document as output
            yield return new object[]
            {
                "Standard valid XML document (V2-YAML)",
                new List<string>
                {
                    Path.Combine(InputDirectory, "Annotation.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi2_0,
                OpenApiFormat.Yaml,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationV2.Json")
            };

            // Standard, original valid XML document with JSON - V2 Open Api Document as output
            yield return new object[]
            {
                "Standard valid XML document (V2-JSON)",
                new List<string>
                {
                    Path.Combine(InputDirectory, "Annotation.xml"),
                    Path.Combine(InputDirectory, "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.xml")
                },
                new List<string>
                {
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.SampleApis.dll"),
                    Path.Combine(
                        InputDirectory,
                        "Microsoft.OpenApi.CSharpComment.Reader.Tests.Contracts.dll")
                },
                OpenApiSpecVersion.OpenApi2_0,
                OpenApiFormat.Json,
                9,
                Path.Combine(
                    OutputDirectory,
                    "AnnotationV2.Json")
            };
        }

        [Theory]
        [MemberData(nameof(GetTestCasesForInvalidDocumentationShouldYieldFailure))]
        public void InvalidDocumentationShouldYieldFailure(
            string testCaseName,
            IList<string> inputXmlFiles,
            IList<string> inputBinaryFiles,
            OpenApiSpecVersion openApiSpecVersion,
            int expectedOperationGenerationResultsCount,
            string expectedJsonFile,
            DocumentGenerationDiagnostic expectedDocumentGenerationResult,
            IList<OperationGenerationDiagnostic> expectedFailureOperationGenerationResults)
        {
            _output.WriteLine(testCaseName);

            var documents = new List<XDocument>();

            documents.AddRange(inputXmlFiles.Select(XDocument.Load));

            var input = new CSharpCommentOpenApiGeneratorConfig(documents, inputBinaryFiles, openApiSpecVersion);

            GenerationDiagnostic result;

            var generator = new CSharpCommentOpenApiGenerator();

            var openApiDocuments = generator.GenerateDocuments(input, out result);

            openApiDocuments.Should().NotBeNull();

            _output.WriteLine(
                JsonConvert.SerializeObject(
                    openApiDocuments.ToSerializedOpenApiDocuments(),
                    new DictionaryJsonConverter<DocumentVariantInfo, string>()));

            result.DocumentGenerationDiagnostic.Should().BeEquivalentTo(expectedDocumentGenerationResult);
            result.OperationGenerationDiagnostics.Count.Should().Be(expectedOperationGenerationResultsCount);

            openApiDocuments[DocumentVariantInfo.Default].Should().NotBeNull();

            var failurePaths = result.OperationGenerationDiagnostics.Where(
                    p => p.Errors.Count > 0)
                .ToList();

            var actualDocument = openApiDocuments[DocumentVariantInfo.Default].SerializeAsJson(openApiSpecVersion);
            var expectedDocument = File.ReadAllText(expectedJsonFile);


            _output.WriteLine(actualDocument);

            failurePaths.Should().BeEquivalentTo(expectedFailureOperationGenerationResults);

            // We are doing serialization and deserialization to force the resulting actual document
            // to have the exact fields we will see in the resulting document based on the contract resolver.
            // Without serialization and deserialization, the actual document may have fields that should
            // not be present, such as empty list fields.
            var openApiStringReader = new OpenApiStringReader();

            var actualDeserializedDocument = openApiStringReader.Read(
                actualDocument,
                out OpenApiDiagnostic diagnostic);

            diagnostic.Errors.Count.Should().Be(0);

            actualDeserializedDocument
                .Should()
                .BeEquivalentTo(openApiStringReader.Read(expectedDocument, out var _));
        }

        [Theory]
        [MemberData(nameof(GetTestCasesForInvalidDocumentationShouldRemoveFailedOperations))]
        public void InvalidDocumentationShouldRemoveFailedOperations(
            string testCaseName,
            IList<string> inputXmlFiles,
            IList<string> inputBinaryFiles,
            OpenApiSpecVersion openApiSpecVersion,
            int expectedOperationGenerationResultsCount,
            string expectedJsonFile,
            DocumentGenerationDiagnostic expectedDocumentGenerationResult,
            IList<OperationGenerationDiagnostic> expectedFailureOperationGenerationResults)
        {
            _output.WriteLine(testCaseName);

            var documents = new List<XDocument>();

            documents.AddRange(inputXmlFiles.Select(XDocument.Load));

            var input = new CSharpCommentOpenApiGeneratorConfig(documents, inputBinaryFiles, openApiSpecVersion);

            GenerationDiagnostic result;

            var generator = new CSharpCommentOpenApiGenerator();

            var openApiDocuments = generator.GenerateDocuments(input, out result);

            result.Should().NotBeNull();

            _output.WriteLine(
                 JsonConvert.SerializeObject(
                     openApiDocuments.ToSerializedOpenApiDocuments(),
                     new DictionaryJsonConverter<DocumentVariantInfo, string>()));

            result.DocumentGenerationDiagnostic.Should().BeEquivalentTo(expectedDocumentGenerationResult);

            openApiDocuments[DocumentVariantInfo.Default].Should().NotBeNull();
            result.OperationGenerationDiagnostics.Count.Should().Be(expectedOperationGenerationResultsCount);

            var failedPaths = result.OperationGenerationDiagnostics.Where(
                    p => p.Errors.Count > 0)
                .ToList();

            var actualDocument = openApiDocuments[DocumentVariantInfo.Default].SerializeAsJson(openApiSpecVersion);
            var expectedDocument = File.ReadAllText(expectedJsonFile);

            _output.WriteLine(actualDocument);

            failedPaths.Should().BeEquivalentTo(expectedFailureOperationGenerationResults);

            // We are doing serialization and deserialization to force the resulting actual document
            // to have the exact fields we will see in the resulting document based on the contract resolver.
            // Without serialization and deserialization, the actual document may have fields that should
            // not be present, such as empty list fields.
            var openApiStringReader = new OpenApiStringReader();

            var actualDeserializedDocument = openApiStringReader.Read(
                actualDocument,
                out OpenApiDiagnostic diagnostic);

            diagnostic.Errors.Count.Should().Be(0);

            actualDeserializedDocument
                .Should()
                .BeEquivalentTo(openApiStringReader.Read(expectedDocument, out var _));
        }

        [Theory]
        [InlineData(OpenApiSpecVersion.OpenApi3_0)]
        public void NoOperationsToParseShouldReturnEmptyDocument(OpenApiSpecVersion openApiSpecVersion)
        {
            var path = Path.Combine(InputDirectory, "AnnotationNoOperationsToParse.xml");

            var document = XDocument.Load(path);

            var input = new CSharpCommentOpenApiGeneratorConfig(new List<XDocument>() {document}, new List<string>(),
                openApiSpecVersion);

            GenerationDiagnostic result;

            var generator = new CSharpCommentOpenApiGenerator();
            var openApiDocument = generator.GenerateDocument(input, out result);

            result.Should().NotBeNull();
            openApiDocument.Should().BeNull();
            result.DocumentGenerationDiagnostic.Should()
                .BeEquivalentTo(
                    new DocumentGenerationDiagnostic
                    {
                        Errors =
                        {
                            new GenerationError
                            {
                                Message = SpecificationGenerationMessages.NoOperationElementFoundToParse,
                            }
                        }
                    }
                );
        }

        [Theory]
        [MemberData(nameof(GetTestCasesForValidDocumentationShouldReturnCorrectDocument))]
        public void ValidDocumentationShouldReturnCorrectDocument(
            string testCaseName,
            IList<string> inputXmlFiles,
            IList<string> inputBinaryFiles,
            OpenApiSpecVersion openApiSpecVersion,
            int expectedOperationGenerationResultsCount,
            string expectedJsonFile)
        {
            _output.WriteLine(testCaseName);

            var documents = new List<XDocument>();

            documents.AddRange(inputXmlFiles.Select(XDocument.Load));

            var input = new CSharpCommentOpenApiGeneratorConfig(documents, inputBinaryFiles, openApiSpecVersion);

            GenerationDiagnostic result;

            var generator = new CSharpCommentOpenApiGenerator();
            var openApiDocuments = generator.GenerateDocuments(input, out result);

            result.Should().NotBeNull();

            _output.WriteLine(
                JsonConvert.SerializeObject(
                    openApiDocuments.ToSerializedOpenApiDocuments(),
                    new DictionaryJsonConverter<DocumentVariantInfo, string>()));

            result.DocumentGenerationDiagnostic.Errors.Count.Should().Be(0);

            openApiDocuments[DocumentVariantInfo.Default].Should().NotBeNull();

            result.OperationGenerationDiagnostics.Where(p => p.Errors.Count > 0).Count().Should().Be(0);
            result.OperationGenerationDiagnostics.Count.Should().Be(expectedOperationGenerationResultsCount);

            var actualDocument = openApiDocuments[DocumentVariantInfo.Default].SerializeAsJson(openApiSpecVersion);
            var expectedDocument = File.ReadAllText(expectedJsonFile);

            _output.WriteLine(actualDocument);

            var openApiStringReader = new OpenApiStringReader();

            var actualDeserializedDocument = openApiStringReader.Read(
                actualDocument,
                out OpenApiDiagnostic diagnostic);

            diagnostic.Errors.Count.Should().Be(0);

            actualDeserializedDocument
                .Should()
                .BeEquivalentTo(openApiStringReader.Read(expectedDocument, out var _));
        }

        [Theory]
        [MemberData(nameof(GetTestCasesForPassANewFilterAndShouldReturnCorrectDocument))]
        public void PassANewFilterAndShouldReturnCorrectDocument(
            string testCaseName,
            IList<string> inputXmlFiles,
            IList<string> inputBinaryFiles,
            IDocumentFilter documentFilter,
            OpenApiSpecVersion openApiSpecVersion,
            int expectedOperationGenerationResultsCount,
            string expectedJsonFile)
        {
            _output.WriteLine(testCaseName);

            var documents = new List<XDocument>();

            documents.AddRange(inputXmlFiles.Select(XDocument.Load));

            var input = new CSharpCommentOpenApiGeneratorConfig(documents, inputBinaryFiles, openApiSpecVersion);
            input.CSharpCommentOpenApiGeneratorFilterConfig.DocumentFilters.Add(documentFilter);

            GenerationDiagnostic result;

            var generator = new CSharpCommentOpenApiGenerator();
            var openApiDocuments = generator.GenerateDocuments(input, out result);

            result.Should().NotBeNull();

            _output.WriteLine(
                JsonConvert.SerializeObject(
                    openApiDocuments.ToSerializedOpenApiDocuments(),
                    new DictionaryJsonConverter<DocumentVariantInfo, string>()));

            _output.WriteLine(JsonConvert.SerializeObject(result));

            result.DocumentGenerationDiagnostic.Errors.Count.Should().Be(0);

            openApiDocuments[DocumentVariantInfo.Default].Should().NotBeNull();

            result.OperationGenerationDiagnostics.Where(p => p.Errors.Count > 0).Count().Should().Be(0);
            result.OperationGenerationDiagnostics.Count.Should().Be(expectedOperationGenerationResultsCount);

            var actualDocument = openApiDocuments[DocumentVariantInfo.Default].SerializeAsJson(openApiSpecVersion);
            var expectedDocument = File.ReadAllText(expectedJsonFile);

            _output.WriteLine(actualDocument);

            var openApiStringReader = new OpenApiStringReader();

            var actualDeserializedDocument = openApiStringReader.Read(
                actualDocument,
                out OpenApiDiagnostic diagnostic);

            diagnostic.Errors.Count.Should().Be(0);

            actualDeserializedDocument
                .Should()
                .BeEquivalentTo(openApiStringReader.Read(expectedDocument, out var _));
        }

        [Theory]
        [MemberData(nameof(GetTestCasesForValidDocumentationShouldReturnCorrectSerializedDocument))]
        public void ValidDocumentationShouldReturnCorrectSerializedDocument(
            string testCaseName,
            IList<string> inputXmlFiles,
            IList<string> inputBinaryFiles,
            OpenApiSpecVersion openApiSpecVersion,
            OpenApiFormat openApiFormat,
            int expectedOperationGenerationResultsCount,
            string expectedJsonFile)
        {
            _output.WriteLine(testCaseName);

            var documents = new List<XDocument>();

            documents.AddRange(inputXmlFiles.Select(XDocument.Load));

            var input = new CSharpCommentOpenApiGeneratorConfig(documents, inputBinaryFiles, openApiSpecVersion)
            {
                OpenApiFormat = openApiFormat
            };

            var generator = new CSharpCommentOpenApiGenerator();

            GenerationDiagnostic result;

            var serializedDocuments = generator.GenerateSerializedDocuments(input, out result);

            result.Should().NotBeNull();
            serializedDocuments.Should().NotBeNull();

            _output.WriteLine(JsonConvert.SerializeObject(serializedDocuments));

            result.DocumentGenerationDiagnostic.Errors.Count.Should().Be(0);

            serializedDocuments[DocumentVariantInfo.Default].Should().NotBeNull();

            result.OperationGenerationDiagnostics.Where(p => p.Errors.Count > 0).Count().Should().Be(0);
            result.OperationGenerationDiagnostics.Count.Should().Be(expectedOperationGenerationResultsCount);

            var actualDocument = serializedDocuments[DocumentVariantInfo.Default];
            var expectedDocument = File.ReadAllText(expectedJsonFile);

            _output.WriteLine(actualDocument);

            var openApiStringReader = new OpenApiStringReader();

            var actualDeserializedDocument = openApiStringReader.Read(
                actualDocument,
                out OpenApiDiagnostic diagnostic);

            diagnostic.Errors.Count.Should().Be(0);

            actualDeserializedDocument
                .Should()
                .BeEquivalentTo(openApiStringReader.Read(expectedDocument, out var _));
        }
    }
}