﻿// This software is part of the Autofac IoC container
// Copyright (c) 2010 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Autofac.Core;
using Autofac.Util;

namespace Autofac.Features.GeneratedFactories
{
    /// <summary>
    /// Generates context-bound closures that represent factories from
    /// a set of heuristics based on delegate type signatures.
    /// </summary>
    public class FactoryGenerator
    {
        readonly Func<IComponentContext, IEnumerable<Parameter>, Delegate> _generator;

        /// <summary>
        /// Create a factory generator.
        /// </summary>
        /// <param name="service">The service that will be activated in
        /// order to create the products of the factory.</param>
        /// <param name="delegateType">The delegate to provide as a factory.</param>
        /// <param name="parameterMapping">The parameter mapping mode to use.</param>
        public FactoryGenerator(Type delegateType, Service service, ParameterMapping parameterMapping)
        {
            Enforce.ArgumentNotNull(service, "service");
            Enforce.ArgumentTypeIsFunction(delegateType);

            _generator = CreateGenerator((activatorContextParam, resolveParameterArray) =>
                {
                    // c, service, [new Parameter(name, (object)dps)]*
                    var resolveParams = new Expression[] {
                            activatorContextParam,
                            Expression.Constant(service),
                            Expression.NewArrayInit(typeof(Parameter), resolveParameterArray)
                        };

                    // c.Resolve(...)
                    return Expression.Call(
                        typeof(ResolutionExtensions).GetMethod("Resolve", new[] { typeof(IComponentContext), typeof(Service), typeof(Parameter[]) }),
                        resolveParams);
                },
                delegateType,
                GetParameterMapping(delegateType, parameterMapping));
        }

        /// <summary>
        /// Create a factory generator.
        /// </summary>
        /// <param name="productRegistration">The component that will be activated in
        /// order to create the products of the factory.</param>
        /// <param name="delegateType">The delegate to provide as a factory.</param>
        /// <param name="parameterMapping">The parameter mapping mode to use.</param>
        public FactoryGenerator(Type delegateType, IComponentRegistration productRegistration, ParameterMapping parameterMapping)
        {
            Enforce.ArgumentNotNull(productRegistration, "productRegistration");
            Enforce.ArgumentTypeIsFunction(delegateType);

            _generator = CreateGenerator((activatorContextParam, resolveParameterArray) =>
                {
                    // productRegistration, [new Parameter(name, (object)dps)]*
                    var resolveParams = new Expression[] {
                        Expression.Constant(productRegistration),
                        Expression.NewArrayInit(typeof(Parameter), resolveParameterArray)
                    };

                    // c.Resolve(...)
                    return Expression.Call(
                        activatorContextParam,
                        typeof(IComponentContext).GetMethod("Resolve", new[] { typeof(IComponentRegistration), typeof(Parameter[]) }),
                        resolveParams);
                },
                delegateType,
                GetParameterMapping(delegateType, parameterMapping));
        }

        ParameterMapping GetParameterMapping(Type delegateType, ParameterMapping configuredParameterMapping)
        {
            if (configuredParameterMapping == ParameterMapping.Adaptive)
                return delegateType.Name.StartsWith("Func`") ? ParameterMapping.ByType : ParameterMapping.ByName;
            else
                return configuredParameterMapping;
        }

        Func<IComponentContext, IEnumerable<Parameter>, Delegate> CreateGenerator(Func<Expression, Expression[], Expression> makeResolveCall, Type delegateType, ParameterMapping pm)
        {
            // (c, p) => ([dps]*) => (drt)Resolve(c, productRegistration, [new NamedParameter(name, (object)dps)]*)

            // (c, p)
            var activatorContextParam = Expression.Parameter(typeof(IComponentContext), "c");
            var activatorParamsParam = Expression.Parameter(typeof(IEnumerable<Parameter>), "p");
            var activatorParams = new[] { activatorContextParam, activatorParamsParam };

            var invoke = delegateType.GetMethod("Invoke");

            // [dps]*
            var creatorParams = invoke
                .GetParameters()
                .Select(pi => Expression.Parameter(pi.ParameterType, pi.Name))
                .ToList();

            Expression[] resolveParameterArray = MapParameters(delegateType, creatorParams, pm);

            var resolveCall = makeResolveCall(activatorContextParam, resolveParameterArray);

            // (drt)
            var resolveCast = Expression.Convert(resolveCall, invoke.ReturnType);

            // ([dps]*) => c.Resolve(service, [new Parameter(name, dps)]*)
            var creator = Expression.Lambda(delegateType, resolveCast, creatorParams);

            // (c, p) => (
            var activator = Expression.Lambda<Func<IComponentContext, IEnumerable<Parameter>, Delegate>>(creator, activatorParams);

            return activator.Compile();
        }

        static Expression[] MapParameters(Type delegateType, List<ParameterExpression> creatorParams, ParameterMapping pm)
        {
            switch (pm)
            {
                case ParameterMapping.ByType:
                    return creatorParams
                            .Select(p => Expression.New(
                                typeof(TypedParameter).GetConstructor(new[] { typeof(Type), typeof(object) }),
                                Expression.Constant(p.Type), Expression.Convert(p, typeof(object))))
                            .OfType<Expression>()
                            .ToArray();

                case ParameterMapping.ByPosition:
                    return creatorParams
                        .Select((p, i) => Expression.New(
                                typeof(PositionalParameter).GetConstructor(new[] { typeof(int), typeof(object) }),
                                Expression.Constant(i), Expression.Convert(p, typeof(object))))
                            .OfType<Expression>()
                            .ToArray();

                case ParameterMapping.ByName:
                default:
                    return creatorParams
                            .Select(p => Expression.New(
                                typeof(NamedParameter).GetConstructor(new[] { typeof(string), typeof(object) }),
                                Expression.Constant(p.Name), Expression.Convert(p, typeof(object))))
                            .OfType<Expression>()
                            .ToArray();
            }
        }

        /// <summary>
        /// Generates a factory delegate that closes over the provided context.
        /// </summary>
        /// <param name="context">The context in which the factory will be used.</param>
        /// <param name="parameters">Parameters provided to the resolve call for the factory itself.</param>
        /// <returns>A factory delegate that will work within the context.</returns>
        public Delegate GenerateFactory(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            Enforce.ArgumentNotNull(context, "context");
            Enforce.ArgumentNotNull(parameters, "parameters");

            return _generator(context.Resolve<ILifetimeScope>(), parameters);
        }

        /// <summary>
        /// Generates a factory delegate that closes over the provided context.
        /// </summary>
        /// <param name="context">The context in which the factory will be used.</param>
        /// <param name="parameters">Parameters provided to the resolve call for the factory itself.</param>
        /// <returns>A factory delegate that will work within the context.</returns>
        public TDelegate GenerateFactory<TDelegate>(IComponentContext context, IEnumerable<Parameter> parameters)
            where TDelegate : class
        {
            return (TDelegate)(object)GenerateFactory(context, parameters);
        }
    }
}