using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

class Program
{
    class Address
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Zip { get; set; }
    }

    class Phone
    {
        public string? Mobile { get; set; }
        public string? Landline { get; set; }
    }

    class Family
    {
        public string? Name { get; set; }
        public string? Born { get; set; }
        public Address? Address { get; set; }
        public Phone? Phone { get; set; }
    }

    class Person
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Address? Address { get; set; }
        public Phone? Phone { get; set; }
        public List<Family> FamilyMembers { get; set; } = new List<Family>();
    }

    static void Main(string[] args)
    {
        string inputFilePath = null;
        string outputFilePath = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-in" && i + 1 < args.Length)
            {
                inputFilePath = args[i + 1];
            }
            else if (args[i] == "-out" && i + 1 < args.Length)
            {
                outputFilePath = args[i + 1];
            }
        }

        if (string.IsNullOrEmpty(inputFilePath) || string.IsNullOrEmpty(outputFilePath))
        {
            Console.WriteLine("Ange sökvägar för både input- och output-filerna med flaggor -in och -out.");
            return;
        }

        List<Person> people = ReadPeople(inputFilePath);
        ConvertToXml(people, outputFilePath);

        Console.WriteLine("Conversion complete.");
    }

    static List<Person> ReadPeople(string inputFilePath)
    {
        List<Person> people = new List<Person>();
        Person currentPerson = null;
        Family currentFamily = null;

        foreach (string line in File.ReadLines(inputFilePath))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] data = line.Split('|');
            string recordType = data[0];

            if (recordType == "P")
            {
                currentPerson = new Person
                {
                    FirstName = data.ElementAtOrDefault(1),
                    LastName = data.ElementAtOrDefault(2)
                };
                currentFamily = null; // Nollställ currentFamily när en ny person kommer in
                people.Add(currentPerson);
            }
            else if (recordType == "T")
            {
                if (currentFamily != null)
                {
                    if (currentFamily.Phone == null)
                        currentFamily.Phone = new Phone();

                    currentFamily.Phone.Mobile = data.ElementAtOrDefault(1);
                    currentFamily.Phone.Landline = data.ElementAtOrDefault(2);
                }
                else
                {
                    currentPerson.Phone = new Phone
                    {
                        Mobile = data.ElementAtOrDefault(1),
                        Landline = data.ElementAtOrDefault(2)
                    };
                }
            }
            else if (recordType == "A")
            {
                if (currentFamily != null)
                {
                    if (currentFamily.Address == null)
                        currentFamily.Address = new Address();

                    currentFamily.Address.Street = data.ElementAtOrDefault(1);
                    currentFamily.Address.City = data.ElementAtOrDefault(2);
                    currentFamily.Address.Zip = data.ElementAtOrDefault(3);
                }
                else
                {
                    currentPerson.Address = new Address
                    {
                        Street = data.ElementAtOrDefault(1),
                        City = data.ElementAtOrDefault(2),
                        Zip = data.ElementAtOrDefault(3)
                    };
                }
            }
            else if (recordType == "F")
            {
                currentFamily = new Family
                {
                    Name = data.ElementAtOrDefault(1),
                    Born = data.ElementAtOrDefault(2)
                };
                currentPerson.FamilyMembers.Add(currentFamily);
            }
        }

        return people;
    }

    static void ConvertToXml(List<Person> people, string outputFilePath)
    {
        XElement peopleElement = new XElement("people");

        foreach (var person in people)
        {
            XElement personElement = new XElement("person",
                new XElement("firstname", person.FirstName),
                new XElement("lastname", person.LastName)
            );

            if (person.Address != null)
            {
                XElement addressElement = new XElement("address",
                    new XElement("street", person.Address.Street),
                    new XElement("city", person.Address.City),
                    new XElement("zip", person.Address.Zip)
                );
                personElement.Add(addressElement);
            }

            if (person.Phone != null)
            {
                XElement phoneElement = new XElement("phone",
                    new XElement("mobile", person.Phone.Mobile),
                    new XElement("landline", person.Phone.Landline)
                );
                personElement.Add(phoneElement);
            }

            foreach (var family in person.FamilyMembers)
            {
                XElement familyElement = new XElement("family",
                    new XElement("name", family.Name),
                    new XElement("born", family.Born)
                );

                if (family.Address != null)
                {
                    XElement addressElement = new XElement("address",
                        new XElement("street", family.Address.Street),
                        new XElement("city", family.Address.City),
                        new XElement("zip", family.Address.Zip)
                    );
                    familyElement.Add(addressElement);
                }

                if (family.Phone != null)
                {
                    XElement phoneElement = new XElement("phone",
                        new XElement("mobile", family.Phone.Mobile),
                        new XElement("landline", family.Phone.Landline)
                    );
                    familyElement.Add(phoneElement);
                }

                personElement.Add(familyElement);
            }

            peopleElement.Add(personElement);
        }

        XDocument xdoc = new XDocument(peopleElement);
        xdoc.Save(outputFilePath);
    }
}
