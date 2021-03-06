// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Testing;
using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class HtmlHelperHiddenTest
    {
        public static IEnumerable<object[]> HiddenWithAttributesData
        {
            get
            {
                var expected1 = @"<input baz=""HtmlEncode[[BazValue]]"" id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" " +
                                @"value=""HtmlEncode[[ModelStateValue]]"" />";
                yield return new object[] { new Dictionary<string, object> { { "baz", "BazValue" } }, expected1 };
                yield return new object[] { new { baz = "BazValue" }, expected1 };

                var expected2 = @"<input foo-baz=""HtmlEncode[[BazValue]]"" id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" " +
                                @"value=""HtmlEncode[[ModelStateValue]]"" />";
                yield return new object[] { new Dictionary<string, object> { { "foo-baz", "BazValue" } }, expected2 };
                yield return new object[] { new { foo_baz = "BazValue" }, expected2 };
            }
        }

        [Fact]
        public void HiddenWithByteArrayValue_GeneratesBase64EncodedValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[ProductName]]"" name=""HtmlEncode[[ProductName]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[Fys1]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();

            // Act
            var result = helper.Hidden("ProductName", new byte[] { 23, 43, 53 }, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [MemberData(nameof(HiddenWithAttributesData))]
        public void HiddenWithArgumentValueAndAttributes_UsesArgumentValue(object attributes, string expected)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewData.Model.Property1 = "should-not-be-used";

            // Act
            var result = helper.Hidden("Property1", "test", attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenNotInTemplate_GetsValueFromPropertyOfViewDataEntry()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Prefix_Property1]]"" name=""HtmlEncode[[Prefix.Property1]]"" " +
                @"type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[contained-view-data-value]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNonNullModel());
            helper.ViewData.Model.Property1 = "model-property1-value";
            helper.ViewData["Prefix"] = new HiddenModel { Property1 = "contained-view-data-value" };

            // Act
            var html = helper.Hidden("Prefix.Property1", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void HiddenInTemplate_GetsValueFromPropertyOfViewDataEntry()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Prefix_Property1]]"" name=""HtmlEncode[[Prefix.Property1]]"" " +
                @"type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[contained-view-data-value]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNonNullModel());
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            helper.ViewData.Model.Property1 = "model-property1-value";
            helper.ViewData["Prefix"] = new HiddenModel { Property1 = "contained-view-data-value" };

            // Act
            var html = helper.Hidden("Property1", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void HiddenNotInTemplate_GetsValueFromViewDataEntry_EvenIfNull()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" " +
                @"type=""HtmlEncode[[hidden]]"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNonNullModel());
            helper.ViewData.Model.Property1 = "model-property1-value";
            helper.ViewData["Property1"] = null;

            // Act
            var html = helper.Hidden("Property1", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void HiddenInTemplate_GetsValueFromViewDataEntry_EvenIfNull()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Prefix_Property1]]"" name=""HtmlEncode[[Prefix.Property1]]"" " +
                @"type=""HtmlEncode[[hidden]]"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNonNullModel());
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            helper.ViewData.Model.Property1 = "model-property1-value";
            helper.ViewData["Prefix.Property1"] = null;

            // Act
            var html = helper.Hidden("Property1", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void HiddenOverridesValueFromAttributesWithArgumentValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[explicit-value]]"" />";
            var attributes = new { value = "attribute-value" };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNullModelAndNonNullViewData());
            helper.ViewData.Clear();

            // Act
            var result = helper.Hidden("Property1", "explicit-value", attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenWithArgumentValueAndNullModel_UsesArgumentValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" key=""HtmlEncode[[value]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" " +
                           @"value=""HtmlEncode[[test]]"" />";
            var attributes = new { key = "value" };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNullModelAndNonNullViewData());

            // Act
            var result = helper.Hidden("Property1", "test", attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenWithNonNullValue_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input data-key=""HtmlEncode[[value]]"" id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" " +
                           @"value=""HtmlEncode[[test]]"" />";
            var attributes = new Dictionary<string, object> { { "data-key", "value" } };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNullModelAndNonNullViewData());

            // Act
            var result = helper.Hidden("Property1", "test", attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenUsesValuesFromModelState_OverExplicitSpecifiedValueAndPropertyValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[ModelStateValue]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewData.Model.Property1 = "test-value";

            // Act
            var result = helper.Hidden("Property1", value: "explicit-value", htmlAttributes: new { value = "attribute-value" });

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenUsesExplicitValue_IfModelStateDoesNotHaveProperty()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[explicit-value]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewData.ModelState.Clear();
            helper.ViewData.Model.Property1 = "property-value";

            // Act
            var result = helper.Hidden("Property1", value: "explicit-value", htmlAttributes: new { value = "attribute-value" });

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenUsesValueFromViewData_IfModelStateDoesNotHavePropertyAndExplicitValueIsNull()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[view-data-val]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewData.ModelState.Clear();
            helper.ViewData.Model.Property1 = "property-value";

            // Act
            var result = helper.Hidden("Property1", value: null, htmlAttributes: new { value = "attribute-value" });

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenNotInTemplate_GetsModelValue_IfModelStateAndViewDataEmpty()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[property-value]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNonNullModel());
            helper.ViewData.Model.Property1 = "property-value";

            // Act
            var result = helper.Hidden("Property1", value: null, htmlAttributes: new { value = "attribute-value" });

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact(Skip = "#1485, unable to get Model value.")]
        public void HiddenInTemplate_GetsModelValue_IfModelStateAndViewDataEmpty()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Prefix_Property1]]"" name=""HtmlEncode[[Prefix.Property1]]"" " +
                @"type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[property-value]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNonNullModel());
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Prefix";
            helper.ViewData.Model.Property1 = "property-value";

            // Act
            var html = helper.Hidden("Property1", value: null, htmlAttributes: new { value = "attribute-value" });

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void HiddenNotInTemplate_DoesNotUseAttributeValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNonNullModel());

            // Act
            var result = helper.Hidden("Property1", value: null, htmlAttributes: new { value = "attribute-value" });

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenInTemplate_DoesNotUseAttributeValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Prefix_Property1]]"" name=""HtmlEncode[[Prefix.Property1]]"" " +
                @"type=""HtmlEncode[[hidden]]"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNonNullModel());
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Prefix";

            // Act
            var html = helper.Hidden("Property1", value: null, htmlAttributes: new { value = "attribute-value" });

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void HiddenNotInTemplate_GetsEmptyValue_IfPropertyIsNotFound()
        {
            // Arrange
            var expected = @"<input baz=""HtmlEncode[[BazValue]]"" id=""HtmlEncode[[keyNotFound]]"" name=""HtmlEncode[[keyNotFound]]"" type=""HtmlEncode[[hidden]]"" " +
                           @"value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            var attributes = new Dictionary<string, object> { { "baz", "BazValue" } };

            // Act
            var result = helper.Hidden("keyNotFound", value: null, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenInTemplate_GetsEmptyValue_IfPropertyIsNotFound()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Prefix_keyNotFound]]"" name=""HtmlEncode[[Prefix.keyNotFound]]"" " +
                @"type=""HtmlEncode[[hidden]]"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Prefix";

            // Act
            var html = helper.Hidden("keyNotFound", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, html.ToString());
        }

        [Fact]
        public void HiddenInTemplate_WithExplicitValue_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[MyPrefix_Property1]]"" name=""HtmlEncode[[MyPrefix.Property1]]"" type=""HtmlEncode[[hidden]]"" " +
                           @"value=""HtmlEncode[[PropValue]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var result = helper.Hidden("Property1", "PropValue", htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenInTemplate_WithExplicitValueAndEmptyName_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[MyPrefix]]"" name=""HtmlEncode[[MyPrefix]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[fooValue]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var result = helper.Hidden(string.Empty, "fooValue", htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenInTemplate_UsesPrefixName_ToLookupPropertyValueInModelState()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[MyPrefix$Property1]]"" name=""HtmlEncode[[MyPrefix.Property1]]"" type=""HtmlEncode[[hidden]]"" " +
                           @"value=""HtmlEncode[[modelstate-with-prefix]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(
                GetViewDataWithModelStateAndModelAndViewDataValues(),
                idAttributeDotReplacement: "$");
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";
            helper.ViewData.ModelState.Clear();
            helper.ViewData.ModelState.Add("Property1", GetModelState("modelstate-without-prefix"));
            helper.ViewData.ModelState.Add("MyPrefix.Property1", GetModelState("modelstate-with-prefix"));
            helper.ViewData.ModelState.Add("MyPrefix$Property1", GetModelState("modelstate-with-iddotreplacement"));

            // Act
            var result = helper.Hidden("Property1", "explicit-value", htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenInTemplate_UsesPrefixNameToLookupPropertyValueInViewData()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[MyPrefix$Property1]]"" name=""HtmlEncode[[MyPrefix.Property1]]"" type=""HtmlEncode[[hidden]]"" " +
                           @"value=""HtmlEncode[[vdd-with-prefix]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(
                GetViewDataWithModelStateAndModelAndViewDataValues(),
                idAttributeDotReplacement: "$");
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";
            helper.ViewData.ModelState.Clear();
            helper.ViewData.Clear();
            helper.ViewData.Add("Property1", "vdd-without-prefix");
            helper.ViewData.Add("MyPrefix.Property1", "vdd-with-prefix");
            helper.ViewData.Add("MyPrefix$Property1", "vdd-with-iddotreplacement");

            // Act
            var result = helper.Hidden("Property1", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenWithEmptyNameAndPrefixThrows()
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            var attributes = new Dictionary<string, object>
            {
                { "class", "some-class"}
            };

            // Act and Assert
            ExceptionAssert.ThrowsArgumentNullOrEmpty(() => helper.Hidden(string.Empty, string.Empty, attributes),
                                                      "expression");
        }

        [Fact]
        public void HiddenWithViewDataErrors_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input baz=""HtmlEncode[[BazValue]]"" class=""HtmlEncode[[input-validation-error some-class]]"" id=""HtmlEncode[[Property1]]""" +
                           @" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[ModelStateValue]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithErrors());
            var attributes = new Dictionary<string, object>
            {
                { "baz", "BazValue" },
                { "class", "some-class"}
            };

            // Act
            var result = helper.Hidden("Property1", value: null, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenGeneratesUnobtrusiveValidation()
        {
            // Arrange
            var expected = @"<input data-val=""HtmlEncode[[true]]"" data-val-required=""HtmlEncode[[The Property2 field is required.]]"" " +
                           @"id=""HtmlEncode[[Property2]]"" name=""HtmlEncode[[Property2]]"" type=""HtmlEncode[[hidden]]"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());

            // Act
            var result = helper.Hidden("Property2", value: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        public static IEnumerable<object[]> HiddenWithComplexExpressions_UsesValueFromViewDataData
        {
            get
            {
                yield return new object[]
                {
                    "Property3[height]",
                    @"<input data-test=""HtmlEncode[[val]]"" id=""HtmlEncode[[Property3_height_]]"" name=""HtmlEncode[[Property3[height]]]"" type=""HtmlEncode[[hidden]]"" " +
                    @"value=""HtmlEncode[[Prop3Value]]"" />",
                };

                yield return new object[]
                {
                    "Property4.Property5",
                    @"<input data-test=""HtmlEncode[[val]]"" id=""HtmlEncode[[Property4_Property5]]"" name=""HtmlEncode[[Property4.Property5]]"" " +
                    @"type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[Prop5Value]]"" />",
                };

                yield return new object[]
               {
                    "Property4.Property6[0]",
                    @"<input data-test=""HtmlEncode[[val]]"" id=""HtmlEncode[[Property4_Property6_0_]]"" name=""HtmlEncode[[Property4.Property6[0]]]"" " +
                    @"type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[Prop6Value]]"" />",
               };
            }
        }

        [Theory]
        [MemberData(nameof(HiddenWithComplexExpressions_UsesValueFromViewDataData))]
        public void HiddenWithComplexExpressions_UsesValueFromViewData(string expression, string expected)
        {
            // Arrange
            var viewData = GetViewDataWithModelStateAndModelAndViewDataValues();
            viewData["Property3[height]"] = "Prop3Value";
            viewData["Property4.Property5"] = "Prop5Value";
            viewData["Property4.Property6[0]"] = "Prop6Value";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(viewData);
            var attributes = new Dictionary<string, object> { { "data-test", "val" } };

            // Act
            var result = helper.Hidden(expression, value: null, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        public static IEnumerable<object[]> HiddenWithComplexExpressions_UsesIdDotSeparatorData
        {
            get
            {
                yield return new object[]
                {
                    "Property4.Property5",
                    @"<input data-test=""HtmlEncode[[val]]"" id=""HtmlEncode[[Property4$$Property5]]"" name=""HtmlEncode[[Property4.Property5]]"" " +
                    @"type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[Prop5Value]]"" />",
                };

                yield return new object[]
               {
                    "Property4.Property6[0]",
                    @"<input data-test=""HtmlEncode[[val]]"" id=""HtmlEncode[[Property4$$Property6$$0$$]]"" name=""HtmlEncode[[Property4.Property6[0]]]"" " +
                    @"type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[Prop6Value]]"" />",
               };
            }
        }

        [Theory]
        [MemberData(nameof(HiddenWithComplexExpressions_UsesIdDotSeparatorData))]
        public void HiddenWithComplexExpressions_UsesIdDotSeparator(string expression, string expected)
        {
            // Arrange
            var viewData = GetViewDataWithModelStateAndModelAndViewDataValues();
            viewData["Property3[height]"] = "Prop3Value";
            viewData["Property4.Property5"] = "Prop5Value";
            viewData["Property4.Property6[0]"] = "Prop6Value";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(viewData, idAttributeDotReplacement: "$$");
            var attributes = new Dictionary<string, object> { { "data-test", "val" } };

            // Act
            var result = helper.Hidden(expression, value: null, htmlAttributes: attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForWithByteArrayValue_GeneratesBase64EncodedValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Bytes]]"" name=""HtmlEncode[[Bytes]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[Fys1]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewData.Model.Bytes = new byte[] { 23, 43, 53 };

            // Act
            var result = helper.HiddenFor(m => m.Bytes, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Theory]
        [MemberData(nameof(HiddenWithAttributesData))]
        public void HiddenForWithAttributes_GeneratesExpectedValue(object htmlAttributes, string expected)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewData.Model.Property1 = "test";

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenFor_UsesModelStateValueOverPropertyValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[ModelStateValue]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewData.Model.Property1 = "DefaultValue";

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenFor_UsesPropertyValueIfModelStateDoesNotHaveKey()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[PropertyValue]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewData.ModelState.Clear();
            helper.ViewData.Model.Property1 = "PropertyValue";

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForDoesNotUseValueFromViewDataDictionary_IfModelStateAndPropertyValueIsNull()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewData.Model.Property1 = null;
            helper.ViewData.ModelState.Clear();

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForWithAttributesDictionaryAndNullModel_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" key=""HtmlEncode[[value]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNullModelAndNonNullViewData());
            var attributes = new Dictionary<string, object> { { "key", "value" } };

            // Act
            var result = helper.HiddenFor(m => m.Property1, attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        // This test ensures that specifying a the prefix does not affect the expression result.
        [Fact]
        public void HiddenForInTemplate_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[MyPrefix_Property1]]"" name=""HtmlEncode[[MyPrefix.Property1]]"" type=""HtmlEncode[[hidden]]"" " +
                           @"value=""HtmlEncode[[propValue]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithModelStateAndModelAndViewDataValues());
            helper.ViewData.Model.Property1 = "propValue";
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForInTemplate_UsesPrefixWhenLookingUpModelStateValues()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[MyPrefix$Property1]]"" name=""HtmlEncode[[MyPrefix.Property1]]"" type=""HtmlEncode[[hidden]]"" " +
                           @"value=""HtmlEncode[[modelstate-with-prefix]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(
                GetViewDataWithModelStateAndModelAndViewDataValues(),
                "$");
            helper.ViewData.Model.Property1 = "propValue";
            helper.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = "MyPrefix";
            helper.ViewData.ModelState.Clear();
            helper.ViewData.ModelState.Add("Property1", GetModelState("modelstate-without-prefix"));
            helper.ViewData.ModelState.Add("MyPrefix.Property1", GetModelState("modelstate-with-prefix"));
            helper.ViewData.ModelState.Add("MyPrefix$Property1", GetModelState("modelstate-with-iddotreplacement"));

            // Act
            var result = helper.HiddenFor(m => m.Property1, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenForWithViewDataErrors_GeneratesExpectedValue()
        {
            // Arrange
            var expected = @"<input baz=""HtmlEncode[[BazValue]]"" class=""HtmlEncode[[input-validation-error some-class]]"" id=""HtmlEncode[[Property1]]"" " +
                           @"name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[ModelStateValue]]"" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithErrors());
            var attributes = new Dictionary<string, object>
            {
                { "baz", "BazValue" },
                { "class", "some-class"}
            };

            // Act
            var result = helper.HiddenFor(m => m.Property1, attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenFor_GeneratesUnobtrusiveValidationAttributes()
        {
            // Arrange
            var expected = @"<input data-val=""HtmlEncode[[true]]"" data-val-required=""HtmlEncode[[The Property2 field is required.]]"" " +
                           @"id=""HtmlEncode[[Property2]]"" name=""HtmlEncode[[Property2]]"" type=""HtmlEncode[[hidden]]"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithErrors());

            // Act
            var result = helper.HiddenFor(m => m.Property2, htmlAttributes: null);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        public static TheoryData HiddenFor_UsesPropertyValueIfModelStateDoesNotContainValueData
        {
            get
            {
                var localModel = new HiddenModel();
                localModel.Property4.Property5 = "local-value";
                return new TheoryData<Expression<Func<HiddenModel, string>>, string>
                {
                    {
                        model => model.Property3["key"],
                        @"<input data-val=""HtmlEncode[[true]]"" id=""HtmlEncode[[Property3_key_]]"" name=""HtmlEncode[[Property3[key]]]"" " +
                        @"type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[ModelProp3Val]]"" />"
                    },
                    {
                        model => model.Property4.Property5,
                        @"<input data-val=""HtmlEncode[[true]]"" id=""HtmlEncode[[Property4_Property5]]"" name=""HtmlEncode[[Property4.Property5]]"" " +
                        @"type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[ModelProp5Val]]"" />"
                    },
                    {
                        model => model.Property4.Property6[0],
                        @"<input data-val=""HtmlEncode[[true]]"" id=""HtmlEncode[[Property4_Property6_0_]]"" name=""HtmlEncode[[Property4.Property6[0]]]"" " +
                        @"type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[ModelProp6Val]]"" />"
                    },
                    {
                        model => localModel.Property4.Property5,
                        @"<input data-val=""HtmlEncode[[true]]"" id=""HtmlEncode[[localModel_Property4_Property5]]"" " +
                        @"name=""HtmlEncode[[localModel.Property4.Property5]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[local-value]]"" />"
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(HiddenFor_UsesPropertyValueIfModelStateDoesNotContainValueData))]
        public void HiddenFor_UsesPropertyValueIfModelStateDoesNotContainValue(
            Expression<Func<HiddenModel, string>> expression,
            string expected)
        {
            // Arrange
            var viewData = GetViewDataWithModelStateAndModelAndViewDataValues();
            viewData["Property3[key]"] = "Prop3Val";
            viewData["Property4.Property5"] = "Prop4Val";
            viewData["Property4.Property6[0]"] = "Prop6Val";
            viewData.Model.Property3["key"] = "ModelProp3Val";
            viewData.Model.Property4.Property5 = "ModelProp5Val";
            viewData.Model.Property4.Property6.Add("ModelProp6Val");

            var helper = DefaultTemplatesUtilities.GetHtmlHelper(viewData);
            var attributes = new Dictionary<string, object>
            {
                { "data-val", "true" },
                { "value", "attr-val" }
            };

            // Act
            var result = helper.HiddenFor(expression, attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        public static TheoryData HiddenFor_UsesModelStateValueForComplexExpressionsData
        {
            get
            {
                return new TheoryData<Expression<Func<HiddenModel, string>>, string>
                {
                    {
                        model => model.Property3["key"],
                        @"<input data-val=""HtmlEncode[[true]]"" id=""HtmlEncode[[pre_Property3_key_]]"" name=""HtmlEncode[[pre.Property3[key]]]"" " +
                        @"type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[Prop3Val]]"" />"
                    },
                    {
                        model => model.Property4.Property5,
                        @"<input data-val=""HtmlEncode[[true]]"" id=""HtmlEncode[[pre_Property4_Property5]]"" name=""HtmlEncode[[pre.Property4.Property5]]"" " +
                        @"type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[Prop5Val]]"" />"
                    },
                    {
                        model => model.Property4.Property6[0],
                        @"<input data-val=""HtmlEncode[[true]]"" id=""HtmlEncode[[pre_Property4_Property6_0_]]"" " +
                        @"name=""HtmlEncode[[pre.Property4.Property6[0]]]"" type=""HtmlEncode[[hidden]]"" value=""HtmlEncode[[Prop6Val]]"" />"
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(HiddenFor_UsesModelStateValueForComplexExpressionsData))]
        public void HiddenForInTemplate_UsesModelStateValueForComplexExpressions(
            Expression<Func<HiddenModel, string>> expression,
            string expected)
        {
            // Arrange
            var viewData = GetViewDataWithNullModelAndNonNullViewData();
            viewData.ModelState.Add("pre.Property3[key]", GetModelState("Prop3Val"));
            viewData.ModelState.Add("pre.Property4.Property5", GetModelState("Prop5Val"));
            viewData.ModelState.Add("pre.Property4.Property6[0]", GetModelState("Prop6Val"));

            var helper = DefaultTemplatesUtilities.GetHtmlHelper(viewData);
            viewData.TemplateInfo.HtmlFieldPrefix = "pre";
            var attributes = new { data_val = "true", value = "attr-val" };

            // Act
            var result = helper.HiddenFor(expression, attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        [Fact]
        public void HiddenFor_DoesNotUseAttributeValue()
        {
            // Arrange
            var expected = @"<input id=""HtmlEncode[[Property1]]"" name=""HtmlEncode[[Property1]]"" type=""HtmlEncode[[hidden]]"" value="""" />";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(GetViewDataWithNullModelAndNonNullViewData());
            var attributes = new Dictionary<string, object>
            {
                { "value", "AttrValue" }
            };

            // Act
            var result = helper.HiddenFor(m => m.Property1, attributes);

            // Assert
            Assert.Equal(expected, result.ToString());
        }

        private static ViewDataDictionary<HiddenModel> GetViewDataWithNullModelAndNonNullViewData()
        {
            return new ViewDataDictionary<HiddenModel>(new EmptyModelMetadataProvider())
            {
                ["Property1"] = "view-data-val",
            };
        }

        private static ViewDataDictionary<HiddenModel> GetViewDataWithNonNullModel()
        {
            var viewData = new ViewDataDictionary<HiddenModel>(new EmptyModelMetadataProvider())
            {
                Model = new HiddenModel(),
            };

            return viewData;
        }

        private static ViewDataDictionary<HiddenModel> GetViewDataWithModelStateAndModelAndViewDataValues()
        {
            var viewData = GetViewDataWithNonNullModel();
            viewData["Property1"] = "view-data-val";
            viewData.ModelState.Add("Property1", GetModelState("ModelStateValue"));

            return viewData;
        }

        private static ViewDataDictionary<HiddenModel> GetViewDataWithErrors()
        {
            var viewData = GetViewDataWithModelStateAndModelAndViewDataValues();
            viewData.ModelState.AddModelError("Property1", "error 1");
            viewData.ModelState.AddModelError("Property1", "error 2");
            return viewData;
        }

        private static ModelState GetModelState(string value)
        {
            return new ModelState
            {
                Value = new ValueProviderResult(value, value, CultureInfo.InvariantCulture)
            };
        }

        public class HiddenModel
        {
            public string Property1 { get; set; }

            public byte[] Bytes { get; set; }

            [Required]
            public string Property2 { get; set; }

            public Dictionary<string, string> Property3 { get; } = new Dictionary<string, string>();

            public NestedClass Property4 { get; } = new NestedClass();
        }

        public class NestedClass
        {
            public string Property5 { get; set; }

            public List<string> Property6 { get; } = new List<string>();
        }
    }
}