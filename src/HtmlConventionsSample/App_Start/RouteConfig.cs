﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using HtmlConventionsSample.Controllers;
using MvcPowerTools.Routing;
using HttpPostAttribute = System.Web.Mvc.HttpPostAttribute;

namespace HtmlConventionsSample
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            
            RoutingConventions.Configure(r => r
                .RegisterController<HomeController>()
                .RegisterController<QueryController>()
                .HomeIs<HomeController>(h => h.Index())
                .DefaultBuilder(a =>
                {
                    var url = a.ActionCall.Controller.ControllerNameWithoutSuffix()+"/"+a.ActionCall.Method.Name;
                    var route = a.CreateRoute(url);
                    if (a.ActionCall.Method.HasCustomAttribute<HttpPostAttribute>())
                    {
                        route.Constraints["method"] = new HttpMethodConstraint("POST");
                    }
                    return new[] {route};
                }));
        }
    }
}
