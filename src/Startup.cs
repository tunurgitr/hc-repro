using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Configuration;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Descriptors.Definitions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Repro
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddInterfaceType<IFoo>()
                .AddInterfaceType<IBar>()
                .AddType<FooType>()
                ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGraphQL();
            });
        }
    }

    public class Query
    {
        public string Name() => "query";
        public Foo Foo() => new Foo();
    }

    
    public class Foo : IFoo, IBar
    {
        public string Name() => "foo";
    }

    public interface IFoo
    {
        string Name();
    }

    public interface IBar
    {
        string Name();
    }

    public class FooType : ObjectType<Foo>
    {
        protected override void Configure(IObjectTypeDescriptor<Foo> descriptor)
        {
            
        }
        protected override void OnBeforeCompleteType(ITypeCompletionContext context, DefinitionBase definition, IDictionary<string, object?> contextData)
        {
            if (definition is ObjectTypeDefinition def)
            {
                for (var i = def.Interfaces.Count - 1; i > -1; --i)
                {
                    if (def.Interfaces[i] is ExtendedTypeReference inter)
                    {
                        if (inter.Type.Type == typeof(IFoo))
                        {
                            def.Interfaces.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }

}
