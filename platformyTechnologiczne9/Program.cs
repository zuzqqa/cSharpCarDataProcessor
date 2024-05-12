using System.Data;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace platformyTechnologiczne9
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Car> myCars = new List<Car>
            {
                new Car("E250", new Engine(1.8, 204, "CGI"), 2009),
                new Car("E350", new Engine(3.5, 292, "CGI"), 2009),
                new Car("A6", new Engine(2.5, 187, "FSI"), 2012),
                new Car("A6", new Engine(2.8, 220, "FSI"), 2012),
                new Car("A6", new Engine(3.0, 295, "TFSI"), 2012),
                new Car("A6", new Engine(2.0, 175, "TDI"), 2011),
                new Car("A6", new Engine(3.0, 309, "TDI"), 2011),
                new Car("S6", new Engine(4.0, 414, "TFSI"), 2012),
                new Car("S8", new Engine(4.0, 513, "TFSI"), 2012)
            };

            LINQueries(myCars);
            SerializeDataSet(myCars);
            createXmlFromLinq(myCars);
            createHTML(myCars);
            changeNames();
        }

        private static void LINQueries(List<Car> myCars)
        {
            var query1 = myCars
                .Where(car => car.Model == "A6")
                .Select(car => new
                {
                    engineType = car.Motor.Model == "TDI" ? "diesel" : "petrol",
                    hppl = car.Motor.HorsePower / car.Motor.Displacement
                });

            var query2 = from car in query1
                         group car by car.engineType into engineTypeGroup
                         select new
                         {
                             engineType = engineTypeGroup.Key,
                             avgHppl = engineTypeGroup.Average(car => car.hppl)
                         };

            foreach (var group in query2)
            {
                Console.WriteLine($"{group.engineType}: {group.avgHppl}");
            }
        }

        private static void SerializeDataSet(List<Car> myCars)
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<Car>), new XmlRootAttribute("cars"));

            using (StreamWriter writer = new StreamWriter("CarsCollection.xml"))
            {
                ser.Serialize(writer, myCars);
            }

            Console.WriteLine("\n-> Serialization completed");

            List<Car> deserializedCars;
            using (StreamReader reader = new StreamReader("CarsCollection.xml"))
            {
                deserializedCars = (List<Car>)ser.Deserialize(reader)!;
            }

            Console.WriteLine("-> Deserialization completed");

            foreach (var car in deserializedCars)
            {
                Console.WriteLine($"Model: {car.Model}, Year: {car.Year}");
                Console.WriteLine($"Engine: Displacement - {car.Motor.Displacement}, HorsePower - {car.Motor.HorsePower}, Model - {car.Motor.Model}");
                Console.WriteLine();
            }

            XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace("ns", "http://www.w3.org/2001/XMLSchema-instance");

            XElement rootNode = XElement.Load("CarsCollection.xml");

            double avgHP = (double)rootNode.XPathEvaluate("sum(Car[not(engine/@model[contains(., 'TDI')])]/engine[HorsePower]/HorsePower) div count(Car[not(engine/@model[contains(., 'TDI')])]/engine[HorsePower])");

            Console.WriteLine($"-> Average power of cars without TDI engines: {avgHP}");
            IEnumerable<XElement> models = rootNode.XPathSelectElements("Car/Model[not(. = following::Model)]");

            Console.WriteLine("-> Unique car models:");
            foreach (var model in models)
            {
                Console.WriteLine(model.Value);
            }
        }

        private static void createXmlFromLinq(List<Car> myCars)
        {
            IEnumerable<XElement> nodes =
                from car in myCars
                select new XElement("Car",
                    new XElement("Model", car.Model),
                    new XElement("Year", car.Year),
                    new XElement("engine",
                        new XAttribute("model", car.Motor.Model),
                        new XElement("Displacement", car.Motor.Displacement),
                        new XElement("HorsePower", car.Motor.HorsePower)
                    )
                );

            XElement rootNode = new XElement("cars",
                new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                new XAttribute(XNamespace.Xmlns + "xsd", "http://www.w3.org/2001/XMLSchema"),
                nodes);

            rootNode.Save("CarsFromLinq.xml");
        }

        private static void createHTML(List<Car> myCars)
        {
            XDocument template = XDocument.Load("template.html");

            XElement table = template.Root.Element("body").Element("table");

            IEnumerable<XElement> rows =
                from car in myCars
                select new XElement("tr",
                    new XElement("td", car.Model),
                    new XElement("td", car.Year),
                    new XElement("td",
                        new XElement("table",
                            new XElement("tr",
                                new XElement("td", "Displacement"),
                                new XElement("td", car.Motor.Displacement)),
                            new XElement("tr",
                                new XElement("td", "HorsePower"),
                                new XElement("td", car.Motor.HorsePower)),
                            new XElement("tr",
                                new XElement("td", "Model"),
                                new XElement("td", car.Motor.Model))
                        )
                    )
                );

            table.Add(rows);

            template.Save("output.html");
        }

        private static void changeNames()
        {
            XDocument doc = XDocument.Load("CarsCollection.xml");

            // Perform modifications
            foreach (var car in doc.Descendants("Car"))
            {
                // Change the name of the horsePower element to hp
                var horsePowerElement = car.Element("engine")?.Element("HorsePower");
                if (horsePowerElement != null)
                {
                    horsePowerElement.Name = "hp";
                }

                var yearElement = car.Element("Year");
                var modelElement = car.Element("Model");
                if (yearElement != null && modelElement != null)
                {
                    modelElement.Add(new XAttribute("year", yearElement.Value));
                    yearElement.Remove();
                }
            }

            doc.Save("ModifiedCarsCollection.xml");

            Console.WriteLine("XML document has been successfully modified and saved.");
        }
    }

    public class Car
    {
        public string Model { get; set; }
        public Engine Motor { get; set; }
        public int Year { get; set; }

        public Car()
        {

        }
        public Car(string model, Engine motor, int year)
        {
            Model = model;
            Motor = motor;
            Year = year;
        }
    }

    public class Engine
    {
        public double Displacement { get; set; }
        public int HorsePower { get; set; }
        public string Model { get; set; }

        public Engine()
        {

        }

        public Engine(double displacement, int horsePower, string model)
        {
            Displacement = displacement;
            HorsePower = horsePower;
            Model = model;
        }
    }
}
