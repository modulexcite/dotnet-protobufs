﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Google.ProtocolBuffers.TestProtos;

namespace Google.ProtocolBuffers {
  [TestFixture]
  public class DynamicMessageTest {

    private ReflectionTester reflectionTester;

    private ReflectionTester extensionsReflectionTester;

    [SetUp]
    public void SetUp() {
      reflectionTester = new ReflectionTester(TestAllTypes.Descriptor, null);
      extensionsReflectionTester = new ReflectionTester(TestAllExtensions.Descriptor, TestUtil.CreateExtensionRegistry());
    }

    [Test]
    public void DynamicMessageAccessors() {
      IBuilder builder = DynamicMessage.CreateBuilder(TestAllTypes.Descriptor);
      reflectionTester.SetAllFieldsViaReflection(builder);
      IMessage message = builder.Build();
      reflectionTester.AssertAllFieldsSetViaReflection(message);
    }

    [Test]
    public void DynamicMessageExtensionAccessors() {
    // We don't need to extensively test DynamicMessage's handling of
    // extensions because, frankly, it doesn't do anything special with them.
    // It treats them just like any other fields.
    IBuilder builder = DynamicMessage.CreateBuilder(TestAllExtensions.Descriptor);
    extensionsReflectionTester.SetAllFieldsViaReflection(builder);
    IMessage message = builder.Build();
    extensionsReflectionTester.AssertAllFieldsSetViaReflection(message);
  }

    [Test]
    public void DynamicMessageRepeatedSetters() {
      IBuilder builder = DynamicMessage.CreateBuilder(TestAllTypes.Descriptor);
      reflectionTester.SetAllFieldsViaReflection(builder);
      reflectionTester.ModifyRepeatedFieldsViaReflection(builder);
      IMessage message = builder.Build();
      reflectionTester.AssertRepeatedFieldsModifiedViaReflection(message);
    }

    [Test]
    public void DynamicMessageDefaults() {
      reflectionTester.AssertClearViaReflection(DynamicMessage.GetDefaultInstance(TestAllTypes.Descriptor));
      reflectionTester.AssertClearViaReflection(DynamicMessage.CreateBuilder(TestAllTypes.Descriptor).Build());
    }

    [Test]
    public void DynamicMessageSerializedSize() {
      TestAllTypes message = TestUtil.GetAllSet();

      IBuilder dynamicBuilder = DynamicMessage.CreateBuilder(TestAllTypes.Descriptor);
      reflectionTester.SetAllFieldsViaReflection(dynamicBuilder);
      IMessage dynamicMessage = dynamicBuilder.Build();

      Assert.AreEqual(message.SerializedSize, dynamicMessage.SerializedSize);
    }

    [Test]
    public void DynamicMessageSerialization() {
      IBuilder builder =  DynamicMessage.CreateBuilder(TestAllTypes.Descriptor);
      reflectionTester.SetAllFieldsViaReflection(builder);
      IMessage message = builder.Build();

      ByteString rawBytes = message.ToByteString();
      TestAllTypes message2 = TestAllTypes.ParseFrom(rawBytes);

      TestUtil.AssertAllFieldsSet(message2);

      // In fact, the serialized forms should be exactly the same, byte-for-byte.
      Assert.AreEqual(TestUtil.GetAllSet().ToByteString(), rawBytes);
    }

    [Test]
    public void DynamicMessageParsing() {
      TestAllTypes.Builder builder = TestAllTypes.CreateBuilder();
      TestUtil.SetAllFields(builder);
      TestAllTypes message = builder.Build();

      ByteString rawBytes = message.ToByteString();

      IMessage message2 = DynamicMessage.ParseFrom(TestAllTypes.Descriptor, rawBytes);
      reflectionTester.AssertAllFieldsSetViaReflection(message2);
    }

    [Test]
    public void DynamicMessageCopy() {
      TestAllTypes.Builder builder = TestAllTypes.CreateBuilder();
      TestUtil.SetAllFields(builder);
      TestAllTypes message = builder.Build();

      DynamicMessage copy = DynamicMessage.CreateBuilder(message).Build();
      reflectionTester.AssertAllFieldsSetViaReflection(copy);
    }
  }
}