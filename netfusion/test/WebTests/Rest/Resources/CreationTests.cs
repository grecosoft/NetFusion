using System;
using System.Collections.Generic;
using FluentAssertions;
using NetFusion.Rest.Client;
using NetFusion.Rest.Resources;
using NetFusion.Rest.Server.Hal;
using WebTests.Rest.Resources.Models;
using Xunit;

namespace WebTests.Rest.Resources
{
    /// <summary>
    /// 
    /// Contains tests for creating resources from models.  This tests the calls
    /// that would exist in WebApi controllers as follow:
    ///
    ///     - The controller injects a service.
    ///     - The controller interacts with the server to return one or move domain entities.
    ///     - Domain entities are mapping into models defining the REST Api.
    ///     - The models are wrapped within resources containing the following:
    /// 
    ///             - Embedded Resources:  Child resources related to the parent resource.
    ///             - Links:  These are link relations specifying how the resource relates
    ///                       to other resources or actions that can be taken on the resource.
    ///             - Embedded Models:  These are models not wrapped as a resource and therefore
    ///                       do not have embedded items or associated links.
    ///
    ///  The HALJsonOutputFormatter checks for returned resources for which link mappings have
    ///  been defined.  The formatter uses the metadata contained with the mappings to add links
    ///  to the returned resources.
    /// 
    /// </summary>
    public class CreationTests
    {
        /// <summary>
        /// A resource is just a wrapped model containing additional resource specified
        /// information such as links and embedded resources. 
        /// </summary>
        [Fact]
        public void Resource_Wraps_Model()
        {
            // Models populated from domain entities.
            var account = new AccountModel
            {
                AccountNumber = "274823742874234232",
                AvailableBalance = 50_000M
            };

            var payment = new PaymentModel
            {
                PaymentId = "P24234234",
                Amount = 2_000M
            };

            // Using Factory method to create root resource and delegate to add embedded resources:
            var resource = HalResource.New(account, instance =>
            {
                instance.EmbedResource(payment.AsResource(), "last-payment");
            });

            resource.Model.Should().BeSameAs(account);
            resource.Embedded.Should().ContainKey("last-payment");

            // Links will be null.  Links are automatically populated by a custom ASP.NET Core OutputFormatter
            // based on mappings configured within an ASP.NET Core WebApi project.
            resource.Links.Should().BeNull();
            
            // Using Extension Method to create root resource:
            var resource2 = account.AsResource();
            resource2.EmbedResource(payment.AsResource(), "last-payment");
            resource2.Model.Should().BeSameAs(account);
            resource2.Embedded.Should().ContainKey("last-payment");
        }
        
        /// <summary>
        /// The following is an example when a set of related embedded resources are
        /// to be returned but when there is no model associated with the parent resource.
        /// </summary>
        [Fact]
        public void EmptyResource_WithEmbedded_Resources()
        {
            // Models populated from domain entities.
            var account = new AccountModel();
            var payment = new PaymentModel();
            
            var resource = HalResource.New(instance =>
            {
                instance.EmbedResource(account.AsResource(), "active-account");
                instance.EmbedResource(payment.AsResource(), "largest-payment");
            });

            resource.ModelValue.Should().BeNull();
            resource.Embedded.Keys.Should().HaveCount(2);
            resource.HasEmbedded("active-account").Should().BeTrue();
            resource.HasEmbedded("largest-payment").Should().BeTrue();
        }

        /// <summary>
        /// The following is an example when a set of related embedded resource or
        /// resources collections are to be returned but when there is no model
        /// associated with the parent resource.
        /// </summary>
        [Fact]
        public void EmptyResource_WithEmbedded_ResourceCollections()
        {
            // Models Populated from domain entities:
            var account = new AccountModel
            {
                AccountNumber = "274823742874234232",
                AvailableBalance = 50_000M
            };
            
            var payments = new[]
            {
                new PaymentModel { PaymentId = "P24234", Amount = 1_250M},
                new PaymentModel { PaymentId = "P82342", Amount = 4_250M},
            };

            var resource = HalResource.New(instance =>
            {
                instance.EmbedResource(account.AsResource(), "account");
                instance.EmbedResources(payments.AsResources(), "last-payments");
            });

            resource.ModelValue.Should().BeNull();
            resource.Embedded.Keys.Should().HaveCount(2);
            resource.Embedded.Should().ContainKeys("account", "last-payments");
        }

        /// <summary>
        /// If a model does not have associated embedded resources and links,
        /// it can be directly embedded within the parent resource without
        /// having to wrapped within a resource.
        /// </summary>
        [Fact]
        public void Models_CanBeEmbedded_IntoResource()
        {
            var account = new AccountModel
            {
                AccountNumber = "274823742874234232",
                AvailableBalance = 50_000M
            };
            
            var payments = new[]
            {
                new PaymentModel { PaymentId = "P24234", Amount = 1_250M},
                new PaymentModel { PaymentId = "P82342", Amount = 4_250M},
            };

            var reminder = new ReminderModel
            {
                Message = "Don't forget to pay on time."
            };

            var resource = HalResource.New(account, instance =>
            {
                instance.EmbedModels(payments, "last-payment");
                instance.EmbedModel(reminder, "new-reminder");
            });

            resource.Model.Should().BeSameAs(account);
            resource.Embedded.Should().HaveCount(2);
            resource.Embedded.Should().ContainKeys("last-payment", "new-reminder");
            resource.Embedded.Should().ContainValues(payments, reminder);
        }
        
        /// <summary>
        /// Embedded names must be specified and convey how the child
        /// resource or model is associated with the parent resource.
        /// </summary>
        [Fact]
        public void EmbeddedName_Must_BeSpecified()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                HalResource.New(instance => { instance.EmbedModel(new PaymentModel(), null); });
            })
            .Message
            .Should()
            .Be("The embedded name for type: WebTests.Rest.Resources.Models.PaymentModel was not specified.");
        }

        /// <summary>
        /// Embedded names must be unique.
        /// </summary>
        [Fact]
        public void EmbeddedNames_Must_BeUnique()
        {
            Assert.Throws<InvalidOperationException>(() =>
                {
                    HalResource.New(instance =>
                    {
                        instance.EmbedModel(new PaymentModel(), "firstItem");
                        instance.EmbedModel(new AccountModel(), "firstItem");
                    });
                })
                .Message
                .Should()
                .Be("The resource already has an embedded value named: firstItem.");
        }

        [Fact]
        public void RestApi_CanHave_EntryResource()
        {
            var entryModel = new EntryPointModel
            {
                Version = "10.00.1",
                ApiDocUrl = "http://api.docs/finance/accounting.html"
            };

            var resource = entryModel.AsResource();
            resource.Links = new Dictionary<string, Link>
            {
                { "payment", new Link { Href = "/api/finance/payments/{id}"} },
                { "account", new Link { Href = "/api/finance/accounts/{id}"} }
            };

            resource.Model.Should().NotBeNull();
            resource.Links.Should().HaveCount(2);

        }
    }
}