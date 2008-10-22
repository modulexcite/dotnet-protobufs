// Protocol Buffers - Google's data interchange format
// Copyright 2008 Google Inc.
// http://code.google.com/p/protobuf/
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Reflection;

namespace Google.ProtocolBuffers.FieldAccess {
  /// <summary>
  /// Accessor for fields representing a non-repeated message value.
  /// </summary>
  internal sealed class SingleMessageAccessor<TMessage, TBuilder> : SinglePrimitiveAccessor<TMessage, TBuilder>
      where TMessage : IMessage<TMessage, TBuilder>
      where TBuilder : IBuilder<TMessage, TBuilder> {

    /// <summary>
    /// The static method to create a builder for the property type. For example,
    /// in a message type "Foo", a field called "bar" might be of type "Baz". This
    /// method is Baz.CreateBuilder.
    /// </summary>
    private readonly Func<IBuilder> createBuilderDelegate;

    internal SingleMessageAccessor(string name) : base(name) {
      MethodInfo createBuilderMethod = ClrType.GetMethod("CreateBuilder", Type.EmptyTypes);
      if (createBuilderMethod == null) {
        throw new ArgumentException("No public static CreateBuilder method declared in " + ClrType.Name);
      }
      createBuilderDelegate = ReflectionUtil.CreateStaticUpcastDelegate(createBuilderMethod);
    }

    /// <summary>
    /// Creates a message of the appropriate CLR type from the given value,
    /// which may already be of the right type or may be a dynamic message.
    /// </summary>
    private object CoerceType(object value) {

      // If it's already of the right type, we're done
      if (ClrType.IsInstanceOfType(value)) {
        return value;
      }
      
      // No... so let's create a builder of the right type, and merge the value in.
      IMessage message = (IMessage) value;
      return CreateBuilder().WeakMergeFrom(message).WeakBuild();
    }

    public override void SetValue(TBuilder builder, object value) {
      base.SetValue(builder, CoerceType(value));
    }

    public override IBuilder CreateBuilder() {
      return createBuilderDelegate();
    }
  }
}