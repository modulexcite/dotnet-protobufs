﻿
using System;
using System.IO;

namespace Google.ProtocolBuffers.Examples.AddressBook
{
  class AddPerson {
    
    /// <summary>
    /// Builds a person based on user input
    /// </summary>
    static Person PromptForAddress(TextReader input, TextWriter output) {
      Person.Builder person = Person.CreateBuilder();

      output.Write("Enter person ID: ");
      person.Id = int.Parse(input.ReadLine());

      output.Write("Enter name: ");
      person.Name = input.ReadLine();

      output.Write("Enter email address (blank for none): ");
      string email = input.ReadLine();
      if (email.Length > 0) {
        person.Email = email;
      }

      while (true) {
        output.Write("Enter a phone number (or leave blank to finish): ");
        string number = input.ReadLine();
        if (number.Length == 0) {
          break;
        }

        Person.Types.PhoneNumber.Builder phoneNumber =
            Person.Types.PhoneNumber.CreateBuilder().SetNumber(number);

        output.Write("Is this a mobile, home, or work phone? ");
        String type = input.ReadLine();
        switch (type) {
          case "mobile":
            phoneNumber.Type = Person.Types.PhoneType.MOBILE;
            break;
          case "home":
            phoneNumber.Type = Person.Types.PhoneType.HOME;
            break;
          case "work":
            phoneNumber.Type = Person.Types.PhoneType.WORK;
            break;
          default:
            output.Write("Unknown phone type. Using default.");
            break;
        }

        person.AddPhone(phoneNumber);
      }
      return person.Build();
    }

    /// <summary>
    /// Entry point - loads an existing addressbook or creates a new one,
    /// then writes it back to the file.
    /// </summary>
    public static int Main(string[] args) {
      if (args.Length != 1) {
        Console.Error.WriteLine("Usage:  AddPerson ADDRESS_BOOK_FILE");
        return -1;
      }

      AddressBook.Builder addressBook = AddressBook.CreateBuilder();

      if (File.Exists(args[0])) {
        using (Stream file = File.OpenRead(args[0])) {
          addressBook.MergeFrom(file);
        }
      } else {
        Console.WriteLine("{0}: File not found. Creating a new file.", args[0]);
      }

      // Add an address.
      addressBook.AddPerson(PromptForAddress(Console.In, Console.Out));

      // Write the new address book back to disk.
      using (Stream output = File.OpenWrite(args[0])) {
        addressBook.Build().WriteTo(output);
      }
      return 0;
    }
  }
}