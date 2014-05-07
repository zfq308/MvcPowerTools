﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using MvcPowerTools.Html.Internals;

namespace MvcPowerTools.Html
{
    public class HtmlConventionsManager
    {
        private static Dictionary<string, HtmlConventionsManager> profiles = new Dictionary<string, HtmlConventionsManager>(1);

        public const string DefaultProfileId = "default";

        public static HtmlConventionsManager CreateProfile(string profile)
        {
            if (!profiles.ContainsKey(profile))
            {
                profiles[profile] = new HtmlConventionsManager();
            }
            return profiles[profile];
        }

        public static HtmlConventionsManager DefaultProfile
        {
            get { return CreateProfile("default"); }
        }

        private static object sync = new object();

        public static HtmlConventionsManager GetProfile(string profile)
        {
            return profiles[profile];
        }

        private const string CacheKey = "mvc-conventions-profile";

        public static void SetCurrentRequestProfile(string profile, HttpContextBase context)
        {
            lock (sync)
            {
                context.Items[CacheKey] = profiles[profile];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="MissingConventionProfileException"></exception>
        /// <param name="context"></param>
        /// <returns></returns>
        public static HtmlConventionsManager GetCurrentRequestProfile(HttpContextBase context)
        {
            var conv = context.Items[CacheKey];
            if (conv == null)
            {
                if (profiles.Count == 1)
                {
                    return profiles.Values.First();
                }
                throw new MissingConventionProfileException();
            }
            return conv as HtmlConventionsManager;
        }

        /// <summary>
        /// Creates and adds registry to collection
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public IDefinedConventions CreateRegistry(string label)
        {
            label.MustNotBeEmpty("label");
            var conv=_conventions[label] = new ConventionsRegistry();
            return conv;
        }

        public HtmlConventionsManager()
        {
            CreateRegistry(EditorKey);
            CreateRegistry(DisplayKey);            
            CreateRegistry(LabelKey);
            CreateRegistry(ValidationKey);
            this.SetDefaults();
        }

        public const string EditorKey = "editors";
        public const string DisplayKey = "displays";
        public const string LabelKey = "labels";
        public const string ValidationKey = "validation";

        public IDefinedConventions Editors
        {
            get
            {
                return _conventions[EditorKey];
            }
        }

        public IDefinedConventions Displays
        {
            get
            {
                return _conventions[DisplayKey];
            }
        }
        
        public IDefinedConventions Labels
        {
            get
            {
                return _conventions[LabelKey];
            }
        }
        
        public IDefinedConventions Validation
        {
            get
            {
                return _conventions[ValidationKey];
            }
        }

        public IDefinedConventions this[string label]
        {
            get
            {
                return _conventions[label];
            }
        }

        private Dictionary<string,IDefinedConventions> _conventions=new Dictionary<string, IDefinedConventions>();

        /// <summary>
        /// Scans for instances of <see cref="HtmlConventionsModule"/> instantiate them and loads them into manager
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static void LoadModules(params Assembly[] assemblies)
        {
            Array.ForEach(assemblies, a => LoadModule(a.GetInstancesOfTypesDerivedFrom<HtmlConventionsModule>().ToArray()));        
        }

        public static void LoadModule(params HtmlConventionsModule[] modules)
        {
            if (profiles.Count == 0)
            {
                CreateProfile(DefaultProfileId);
            }
            foreach (var module in modules.OrderBy(d=>d.Order))
            {
                if (module.Profile.IsNullOrEmpty())
                {
                    profiles.Values.ForEach(v=>module.Configure(v));
                }
                else
                {
                    var convention =  GetProfile(module.Profile);
                    module.Configure(convention);
                }
                
            }
        }
    }
}