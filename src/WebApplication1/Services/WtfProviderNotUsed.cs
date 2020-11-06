using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing.Template;
using Tochka.JsonRpc.Server.Models;
using Tochka.JsonRpc.Server.Services;

namespace WebApplication1.Services
{
    public class WtfProviderNotUsed : IApiDescriptionProvider
    {
        private readonly IMethodMatcher methodMatcher;
        private readonly IModelMetadataProvider modelMetadataProvider;

        public WtfProviderNotUsed(IMethodMatcher methodMatcher, IModelMetadataProvider modelMetadataProvider)
        {
            this.methodMatcher = methodMatcher;
            this.modelMetadataProvider = modelMetadataProvider;
        }

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var action in context.Actions.OfType<ControllerActionDescriptor>())
            {
                if (action.AttributeRouteInfo != null && action.AttributeRouteInfo.SuppressPathMatching)
                {
                    continue;
                }

                var extensionData = action.GetProperty<ApiDescriptionActionData>();
                var methodMetadata = action.GetProperty<MethodMetadata>();
                if (extensionData != null && methodMetadata != null)
                {

                    context.Results.Add(CreateApiDescription(action, extensionData.GroupName, methodMetadata));
                }
            }
        }

        private ApiDescription CreateApiDescription(ControllerActionDescriptor action, string groupName, MethodMetadata methodMetadata)
        {
            var apiDescription = new ApiDescription
            {
                ActionDescriptor = action,
                GroupName = groupName,
                HttpMethod = HttpMethods.Post,
            };

            var parsedTemplate = TemplateParser.Parse(action.AttributeRouteInfo.Template);
            apiDescription.RelativePath = GetRelativePathWithHash(parsedTemplate, methodMetadata);

            var templateParameters = parsedTemplate?.Parameters?.ToList() ?? new List<TemplatePart>();
            var parameterContext = new ApiParameterContext(modelMetadataProvider, action, templateParameters);

            return apiDescription;
        }

        private string GetRelativePathWithHash(RouteTemplate parsedTemplate, MethodMetadata methodMetadata)
        {
            if (parsedTemplate == null)
            {
                return null;
            }

            var segments = new List<string>();

            foreach (var segment in parsedTemplate.Segments)
            {
                var currentSegment = string.Empty;
                foreach (var part in segment.Parts)
                {
                    if (part.IsLiteral)
                    {
                        currentSegment += part.Text;
                    }
                    else if (part.IsParameter)
                    {
                        currentSegment += "{" + part.Name + "}";
                    }
                }

                segments.Add(currentSegment);
            }

            var methodName = methodMatcher.GetActionName(methodMetadata);
            return string.Join("/", segments) + $"#{methodName}";
        }

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
        }

        public int Order => -700;
    }
}