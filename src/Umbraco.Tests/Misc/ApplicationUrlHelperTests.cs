﻿using System;
using System.Configuration;
using System.Linq;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Sync;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Cache.DistributedCache;
using Umbraco.Tests.TestHelpers.Stubs;


namespace Umbraco.Tests.Misc
{
    [TestFixture]
    public class ApplicationUrlHelperTests
    {
        // note: in tests, read appContext._umbracoApplicationUrl and not the property,
        // because reading the property does run some code, as long as the field is null.

        [TearDown]
        public void Reset()
        {
            Current.Reset();
        }

        [Test]
        public void NoApplicationUrlByDefault()
        {
            var state = new RuntimeState(Mock.Of<ILogger>(), new Lazy<IServerRegistrar>(Mock.Of<IServerRegistrar>), new Lazy<MainDom>(Mock.Of<MainDom>), Mock.Of<IUmbracoSettingsSection>(), Mock.Of<IGlobalSettings>());
            Assert.IsNull(state.ApplicationUrl);
        }

        [Test]
        public void SetApplicationUrlViaServerRegistrar()
        {
            // no applicable settings, but a provider

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string)null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>());

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            var registrar = new Mock<IServerRegistrar>();
            registrar.Setup(x => x.GetCurrentServerUmbracoApplicationUrl()).Returns("http://server1.com/umbraco");

            var state = new RuntimeState(
                Mock.Of<ILogger>(),
                new Lazy<IServerRegistrar>(() => registrar.Object),
                new Lazy<MainDom>(Mock.Of<MainDom>), settings, globalConfig.Object);

            state.EnsureApplicationUrl();

            Assert.AreEqual("http://server1.com/umbraco", state.ApplicationUrl.ToString());
        }

        [Test]
        public void SetApplicationUrlViaProvider()
        {
            // no applicable settings, but a provider

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>());

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            ApplicationUrlHelper.ApplicationUrlProvider = request => "http://server1.com/umbraco";

            

            var state = new RuntimeState(Mock.Of<ILogger>(), new Lazy<IServerRegistrar>(Mock.Of<IServerRegistrar>), new Lazy<MainDom>(Mock.Of<MainDom>), settings, globalConfig.Object);

            state.EnsureApplicationUrl();

            Assert.AreEqual("http://server1.com/umbraco", state.ApplicationUrl.ToString());
        }

        [Test]
        public void SetApplicationUrlWhenNoSettings()
        {
            // no applicable settings, cannot set url

            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>());

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object, Mock.Of<IServerRegistrar>());

            // still NOT set
            Assert.IsNull(url);
        }
        
        [Test]
        public void SetApplicationUrlFromStSettingsNoSsl()
        {
            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == "mycoolhost.com/umbraco"));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(false);

            
            
            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object, Mock.Of<IServerRegistrar>());

            Assert.AreEqual("http://mycoolhost.com/umbraco", url);
        }

        [Test]
        public void SetApplicationUrlFromStSettingsSsl()
        {
            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == (string) null)
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == "mycoolhost.com/umbraco/"));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            
            
            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object, Mock.Of<IServerRegistrar>());

            Assert.AreEqual("https://mycoolhost.com/umbraco", url);
        }

        [Test]
        public void SetApplicationUrlFromWrSettingsSsl()
        {
            var settings = Mock.Of<IUmbracoSettingsSection>(section =>
                section.WebRouting == Mock.Of<IWebRoutingSection>(wrSection => wrSection.UmbracoApplicationUrl == "httpx://whatever.com/umbraco/")
                && section.ScheduledTasks == Mock.Of<IScheduledTasksSection>(tasksSection => tasksSection.BaseUrl == "mycoolhost.com/umbraco"));

            var globalConfig = Mock.Get(SettingsForTests.GenerateMockGlobalSettings());
            globalConfig.Setup(x => x.UseHttps).Returns(true);

            

            var url = ApplicationUrlHelper.TryGetApplicationUrl(settings, Mock.Of<ILogger>(), globalConfig.Object, Mock.Of<IServerRegistrar>());

            Assert.AreEqual("httpx://whatever.com/umbraco", url);
        }

        
    }
}
