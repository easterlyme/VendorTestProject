using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace VendorTestProject.Extensions
{
    public static class AppBuilderExtensions
    {
        public static IApplicationBuilder UseWhen(this IApplicationBuilder app, Func<HttpContext, bool> condition, Action<IApplicationBuilder> configuration)
        {
            var builder = app.New();
            configuration(builder);

            return app.Use(next =>
            {
                builder.Run(next);

                var branch = builder.Build();

                return context =>
                {
                    if (condition(context))
                    {
                        return branch(context);
                    }

                    return next(context);
                };
            });
        }
    }
}
