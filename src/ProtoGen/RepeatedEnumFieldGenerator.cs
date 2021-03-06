using Google.ProtocolBuffers.DescriptorProtos;
using Google.ProtocolBuffers.Descriptors;

namespace Google.ProtocolBuffers.ProtoGen {
  internal class RepeatedEnumFieldGenerator : FieldGeneratorBase, IFieldSourceGenerator {

    internal RepeatedEnumFieldGenerator(FieldDescriptor descriptor)
      : base(descriptor) {
    }

    public void GenerateMembers(TextGenerator writer) {
      if (Descriptor.IsPacked && Descriptor.File.Options.OptimizeFor == FileOptions.Types.OptimizeMode.SPEED) {
        writer.WriteLine("private int {0}MemoizedSerializedSize;", Name);
      }
      writer.WriteLine("private pbc::PopsicleList<{0}> {1}_ = new pbc::PopsicleList<{0}>();", TypeName, Name);
      writer.WriteLine("public scg::IList<{0}> {1}List {{", TypeName, PropertyName);
      writer.WriteLine("  get {{ return pbc::Lists.AsReadOnly({0}_); }}", Name);
      writer.WriteLine("}");

      // TODO(jonskeet): Redundant API calls? Possibly - include for portability though. Maybe create an option.
      writer.WriteLine("public int {0}Count {{", PropertyName);
      writer.WriteLine("  get {{ return {0}_.Count; }}", Name);
      writer.WriteLine("}");

      writer.WriteLine("public {0} Get{1}(int index) {{", TypeName, PropertyName);
      writer.WriteLine("  return {0}_[index];", Name);
      writer.WriteLine("}");
    }

    public void GenerateBuilderMembers(TextGenerator writer) {
      // Note:  We can return the original list here, because we make it unmodifiable when we build
      writer.WriteLine("public scg::IList<{0}> {1}List {{", TypeName, PropertyName);
      writer.WriteLine("  get {{ return result.{0}_; }}", Name);
      writer.WriteLine("}");
      writer.WriteLine("public int {0}Count {{", PropertyName);
      writer.WriteLine("  get {{ return result.{0}Count; }}", PropertyName);
      writer.WriteLine("}");
      writer.WriteLine("public {0} Get{1}(int index) {{", TypeName, PropertyName);
      writer.WriteLine("  return result.Get{0}(index);", PropertyName);
      writer.WriteLine("}");
      writer.WriteLine("public Builder Set{0}(int index, {1} value) {{", PropertyName, TypeName);
      writer.WriteLine("  result.{0}_[index] = value;", Name);
      writer.WriteLine("  return this;");
      writer.WriteLine("}");
      writer.WriteLine("public Builder Add{0}({1} value) {{", PropertyName, TypeName);
      writer.WriteLine("  result.{0}_.Add(value);", Name, TypeName);
      writer.WriteLine("  return this;");
      writer.WriteLine("}");
      writer.WriteLine("public Builder AddRange{0}(scg::IEnumerable<{1}> values) {{", PropertyName, TypeName);
      writer.WriteLine("  base.AddRange(values, result.{0}_);", Name);
      writer.WriteLine("  return this;");
      writer.WriteLine("}");
      writer.WriteLine("public Builder Clear{0}() {{", PropertyName);
      writer.WriteLine("  result.{0}_.Clear();", Name);
      writer.WriteLine("  return this;");
      writer.WriteLine("}");
    }

    public void GenerateMergingCode(TextGenerator writer) {
      writer.WriteLine("if (other.{0}_.Count != 0) {{", Name);
      writer.WriteLine("  base.AddRange(other.{0}_, result.{0}_);", Name);
      writer.WriteLine("}");
    }

    public void GenerateBuildingCode(TextGenerator writer) {
      writer.WriteLine("result.{0}_.MakeReadOnly();", Name);
    }

    public void GenerateParsingCode(TextGenerator writer) {
        // If packed, set up the while loop
      if (Descriptor.IsPacked) {
        writer.WriteLine("int length = input.ReadInt32();");
        writer.WriteLine("int oldLimit = input.PushLimit(length);");
        writer.WriteLine("while (!input.ReachedLimit) {");
        writer.Indent();
      }

      // Read and store the enum
      // TODO(jonskeet): Make a more efficient way of doing this
      writer.WriteLine("int rawValue = input.ReadEnum();");
      writer.WriteLine("if (!global::System.Enum.IsDefined(typeof({0}), rawValue)) {{", TypeName);
      writer.WriteLine("  if (unknownFields == null) {"); // First unknown field - create builder now
      writer.WriteLine("    unknownFields = pb::UnknownFieldSet.CreateBuilder(this.UnknownFields);");
      writer.WriteLine("  }");
      writer.WriteLine("  unknownFields.MergeVarintField({0}, (ulong) rawValue);", Number);
      writer.WriteLine("} else {");
      writer.WriteLine("  Add{0}(({1}) rawValue);", PropertyName, TypeName);
      writer.WriteLine("}");
      
      if (Descriptor.IsPacked) {
        writer.Outdent();
        writer.WriteLine("}");
        writer.WriteLine("input.PopLimit(oldLimit);");
      }
    }

    public void GenerateSerializationCode(TextGenerator writer) {
      writer.WriteLine("if ({0}_.Count > 0) {{", Name);
      writer.Indent();
      if (Descriptor.IsPacked) {
        writer.WriteLine("output.WriteRawVarint32({0});", WireFormat.MakeTag(Descriptor));
        writer.WriteLine("output.WriteRawVarint32((uint) {0}MemoizedSerializedSize);", Name);
        writer.WriteLine("foreach (int element in {0}_) {{", Name);
        writer.WriteLine("  output.WriteEnumNoTag(element);");
        writer.WriteLine("}");
      } else {
        writer.WriteLine("foreach (int element in {0}_) {{", Name);
        writer.WriteLine("  output.WriteEnum({0}, element);", Number);
        writer.WriteLine("}");
      }
      writer.Outdent();
      writer.WriteLine("}");
    }

    public void GenerateSerializedSizeCode(TextGenerator writer) {
      writer.WriteLine("{");
      writer.Indent();
      writer.WriteLine("int dataSize = 0;");
      writer.WriteLine("if ({0}_.Count > 0) {{", Name);
      writer.Indent();
      writer.WriteLine("foreach ({0} element in {1}_) {{", TypeName, Name);
      writer.WriteLine("  dataSize += pb::CodedOutputStream.ComputeEnumSizeNoTag((int) element);");
      writer.WriteLine("}");
      writer.WriteLine("size += dataSize;");
      int tagSize = CodedOutputStream.ComputeTagSize(Descriptor.FieldNumber);
      if (Descriptor.IsPacked) {
        writer.WriteLine("size += {0};", tagSize);
        writer.WriteLine("size += pb::CodedOutputStream.ComputeRawVarint32Size((uint) dataSize);");
      } else {
        writer.WriteLine("size += {0} * {1}_.Count;", tagSize, Name);
      }
      writer.Outdent();
      writer.WriteLine("}");
      // cache the data size for packed fields.
      if (Descriptor.IsPacked) {
        writer.WriteLine("{0}MemoizedSerializedSize = dataSize;", Name);
      }
      writer.Outdent();
      writer.WriteLine("}");
    }
  }
}
