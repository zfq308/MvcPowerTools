﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace MvcPowerTools.Routing.Conventions
{
    /// <summary>
    /// Handler convention, the controller contains 1 GET and 1 POST
    /// All GET methods should be like get(param|param=value). IF param hasn't a default value it's considered required.
    /// A parameter with default value of its type, it's considered optional
    /// POST method should be named just 'post'
    /// </summary>
    public class HandlerRouteConvention:IBuildRoutes
    {
      
        public virtual bool Match(ActionCall actionCall)
        {
            return true;
        }

        public IEnumerable<Route> Build(RouteBuilderInfo info)
        {
            
            var sb = new StringBuilder();
            var name = info.ActionCall.Controller.ControllerNameWithoutSuffix();
            sb.Append(name.ToLowerInvariant());
            
            var defaults = info.CreateDefaults();
            var constraints = new RouteValueDictionary();
            
            if (info.ActionCall.Method.Name.StartsWith("get", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var param in info.ActionCall.Method.GetParameters().Where(d=>!d.ParameterType.IsUserDefinedClass()).Select(d=>d.Name))
                {
                    sb.Append("/{").Append(param).Append("}");
                }
                info.ActionCall.SetParamsDefaults(defaults);
            }


            var httpMethod = info.ActionCall.Method.Name.StartsWith("post",
                                                           StringComparison.OrdinalIgnoreCase)
                                 ? "POST"
                                 : "GET";
            constraints["httpMethod"] =new HttpMethodConstraint(httpMethod) ;
            return new[]{ new Route(sb.ToString(),defaults,constraints,info.Settings.CreateHandler())};
        }
    }
    
}