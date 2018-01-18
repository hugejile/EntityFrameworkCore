// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Castle.DynamicProxy;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ProxyFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly ProxyFactory Instance = new ProxyFactory();

        private readonly ProxyGenerator _generator = new ProxyGenerator();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object Create(
            [NotNull] DbContext context,
            [NotNull] Type entityClrType,
            [NotNull] params object[] constructorArguments)
        {
            var options = context.GetService<IDbContextOptions>().FindExtension<ProxiesOptionsExtension>();

            if (options?.UseLazyLoadingProxies != true)
            {
                throw new InvalidOperationException(ProxiesStrings.ProxiesNotEnabled(entityClrType.ShortDisplayName()));
            }

            var entityType = context.Model.FindRuntimeEntityType(entityClrType);
            if (entityType == null)
            {
                throw new InvalidOperationException(CoreStrings.EntityTypeNotFound(entityClrType.ShortDisplayName()));
            }

            return CreateLazyLoader(entityType, context.GetService<ILazyLoader>(), constructorArguments);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual object CreateLazyLoader(
            [NotNull] IEntityType entityType,
            [NotNull] ILazyLoader loader,
            [NotNull] object[] constructorArguments)
        {
            var proxy = _generator.CreateClassProxy(
                entityType.ClrType,
                ProxyGenerationOptions.Default,
                constructorArguments,
                new LazyLoadingInterceptor(entityType, loader));

            return proxy;
        }
    }
}
