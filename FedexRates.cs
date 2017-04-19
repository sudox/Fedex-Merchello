// This code was built using Visual Studio 2005
using System;
using System.Web.Services.Protocols;
using RateAvailableServiceWebServiceClient.RateServiceWebReference;
using System.Collections.Generic;
using System.Globalization;

// Sample code to call the FedEx Rate Available Services Web Service
// Tested with Microsoft Visual Studio 2005 Professional Edition

namespace RateAvailableServiceWebServiceClient
{
    public class FedexRate
    {
        public static List<string> Rates(string street, string city, string state, string postal, string country, Decimal weight)
        {
            RateRequest request = CreateRateRequest(street, city, state, postal, country, weight);

            RateService service = new RateService();

            List<string> serviceCost = new List<string>();

            try
            {
                RateReply reply = service.getRates(request);

                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    serviceCost = ShowRateReply(reply);
                }
            }

            catch (Exception e)
            {
                return null;
            }

            return serviceCost;
        }

        public static void Main(string[] args)
        {
            List<string> rates = Rates("123 Test Road", "Medina", "OH", "44256", "US", 1M);
            foreach (var r in rates)
            {
                Console.WriteLine(r);
            }
            Console.WriteLine("Press any key to quit!");
            Console.ReadKey();
        }

        private static RateRequest CreateRateRequest(string street, string city, string state, string postal, string country, Decimal weight)
        {
            // Build the RateRequest
            RateRequest request = new RateRequest();
            //
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = "XXX"; // Replace "XXX" with the Key
            request.WebAuthenticationDetail.UserCredential.Password = "XXX"; // Replace "XXX" with the Password
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = "XXX"; // Replace "XXX" with the client's account number
            request.ClientDetail.MeterNumber = "XXX"; // Replace "XXX" with the client's meter number
            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = "XXX"; // This is a reference field for the customer.  Any value can be used and will be provided in the response.
            //
            request.Version = new VersionId();
            //
            request.ReturnTransitAndCommit = true;
            request.ReturnTransitAndCommitSpecified = true;
            //
            SetShipmentDetails(request, street, city, state, postal, country, weight);
            //
            return request;
        }

        private static void SetShipmentDetails(RateRequest request, string street, string city, string state, string postal, string country, Decimal weight)
        {
            request.RequestedShipment = new RequestedShipment();
            request.RequestedShipment.ShipTimestamp = DateTime.Now; // Shipping date and time
            request.RequestedShipment.ShipTimestampSpecified = true;
            request.RequestedShipment.DropoffType = DropoffType.REGULAR_PICKUP; //Drop off types are BUSINESS_SERVICE_CENTER, DROP_BOX, REGULAR_PICKUP, REQUEST_COURIER, STATION
            request.RequestedShipment.DropoffTypeSpecified = true;
            request.RequestedShipment.PackagingType = PackagingType.YOUR_PACKAGING;
            request.RequestedShipment.PackagingTypeSpecified = true;
            //
            SetOrigin(request);
            //
            SetDestination(request, street, city, state, postal, country);
            //
            SetPackageLineItems(request, weight);
            //
            request.RequestedShipment.PackageCount = "1";
            //set to true to request COD shipment
            bool isCodShipment = false;
            if (isCodShipment)
                SetCOD(request);
        }

        private static void SetOrigin(RateRequest request)
        {
            request.RequestedShipment.Shipper = new Party();
            request.RequestedShipment.Shipper.Address = new Address();
            request.RequestedShipment.Shipper.Address.StreetLines = new string[1] { "123 From Road" };
            request.RequestedShipment.Shipper.Address.City = "Cleveland";
            request.RequestedShipment.Shipper.Address.StateOrProvinceCode = "OH";
            request.RequestedShipment.Shipper.Address.PostalCode = "44102";
            request.RequestedShipment.Shipper.Address.CountryCode = "US";
        }

        private static void SetDestination(RateRequest request, string street, string city, string state, string postal, string country)
        {
            request.RequestedShipment.Recipient = new Party();
            request.RequestedShipment.Recipient.Address = new Address();
            request.RequestedShipment.Recipient.Address.StreetLines = new string[1] { street };
            request.RequestedShipment.Recipient.Address.City = city;
            request.RequestedShipment.Recipient.Address.StateOrProvinceCode = state;
            request.RequestedShipment.Recipient.Address.PostalCode = postal;
            request.RequestedShipment.Recipient.Address.CountryCode = country;
        }       

        private static void SetPackageLineItems(RateRequest request, Decimal w)
        {
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[1];
            request.RequestedShipment.RequestedPackageLineItems[0] = new RequestedPackageLineItem();
            request.RequestedShipment.RequestedPackageLineItems[0].SequenceNumber = "1";
            request.RequestedShipment.RequestedPackageLineItems[0].GroupPackageCount = "1";
            //// package weight
            request.RequestedShipment.RequestedPackageLineItems[0].Weight = new Weight();
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Units = WeightUnits.LB;
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.UnitsSpecified = true;
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.Value = w;
            request.RequestedShipment.RequestedPackageLineItems[0].Weight.ValueSpecified = true;
            //// package dimensions
            //request.RequestedShipment.RequestedPackageLineItems[0].Dimensions = new Dimensions();
            //request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Length = "12";
            //request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Width = "13";
            //request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Height = "14";
            //request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.Units = LinearUnits.IN;
            //request.RequestedShipment.RequestedPackageLineItems[0].Dimensions.UnitsSpecified = true;
        }

        private static void SetCOD(RateRequest request)
        {
            // To get all COD rates, set both COD details at both package and shipment level
            // Set COD at Package level for Ground Services
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested = new PackageSpecialServicesRequested();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SpecialServiceTypes = new PackageSpecialServiceType[1];
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.SpecialServiceTypes[0] = PackageSpecialServiceType.COD;
            //
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail = new CodDetail();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CollectionType = CodCollectionType.GUARANTEED_FUNDS;
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CodCollectionAmount = new Money();
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CodCollectionAmount.Amount = 250;
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CodCollectionAmount.AmountSpecified = true;
            request.RequestedShipment.RequestedPackageLineItems[0].SpecialServicesRequested.CodDetail.CodCollectionAmount.Currency = "USD";
            // Set COD at Shipment level for Express Services
            request.RequestedShipment.SpecialServicesRequested = new ShipmentSpecialServicesRequested(); // Special service requested
            request.RequestedShipment.SpecialServicesRequested.SpecialServiceTypes = new ShipmentSpecialServiceType[1];
            request.RequestedShipment.SpecialServicesRequested.SpecialServiceTypes[0] = ShipmentSpecialServiceType.COD;
            //
            request.RequestedShipment.SpecialServicesRequested.CodDetail = new CodDetail();
            request.RequestedShipment.SpecialServicesRequested.CodDetail.CodCollectionAmount = new Money();
            request.RequestedShipment.SpecialServicesRequested.CodDetail.CodCollectionAmount.Amount = 150;
            request.RequestedShipment.SpecialServicesRequested.CodDetail.CodCollectionAmount.AmountSpecified = true;
            request.RequestedShipment.SpecialServicesRequested.CodDetail.CodCollectionAmount.Currency = "USD";
            request.RequestedShipment.SpecialServicesRequested.CodDetail.CollectionType = CodCollectionType.GUARANTEED_FUNDS;// ANY, CASH, GUARANTEED_FUNDS
        }

        private static List<string> ShowRateReply(RateReply reply)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            List<string> rates = new List<string>();
            Console.WriteLine("RateReply details:");
            for (int i = 0; i < reply.RateReplyDetails.Length;i++ )
            {
                RateReplyDetail rateReplyDetail = reply.RateReplyDetails[i];
                //Console.WriteLine("Rate Reply Detail for Service {0} ", i+1);
                if(rateReplyDetail.ServiceTypeSpecified)
                    rates.Add(textInfo.ToTitleCase(textInfo.ToLower(rateReplyDetail.ServiceType.ToString().Replace("_", " "))));
                //if(rateReplyDetail.PackagingTypeSpecified)
                    //Console.WriteLine("Packaging Type: {0}" , rateReplyDetail.PackagingType);

                for (int j = 0; j < rateReplyDetail.RatedShipmentDetails.Length;j++ )
                {
                    RatedShipmentDetail shipmentDetail = rateReplyDetail.RatedShipmentDetails[j];
                    rates.Add(((Convert.ToDecimal(ShowShipmentRateDetails(shipmentDetail)) * 1.23M) + 5.50M).ToString());
                    //ShowPackageRateDetails(shipmentDetail.RatedPackages);
                }
                //ShowDeliveryDetails(rateReplyDetail);
                rates.Add("*");
            }
            return rates;
        }

        private static string ShowShipmentRateDetails(RatedShipmentDetail shipmentDetail)
        {
            Decimal rate;
            if (shipmentDetail == null)
                return "*";
            if (shipmentDetail.ShipmentRateDetail == null)
                return "*";
            ShipmentRateDetail rateDetail = shipmentDetail.ShipmentRateDetail;
            // Console.WriteLine("--- Shipment Rate Detail ---");
            ////
            //Console.WriteLine("RateType: {0} ", rateDetail.RateType);
            //if (rateDetail.TotalBillingWeight != null) Console.WriteLine("Total Billing Weight: {0} {1}", rateDetail.TotalBillingWeight.Value, shipmentDetail.ShipmentRateDetail.TotalBillingWeight.Units);
            if (rateDetail.TotalBaseCharge != null)
                rate = rateDetail.TotalBaseCharge.Amount;
            else
                return "*";
            //if (rateDetail.TotalFreightDiscounts != null) Console.WriteLine("Total Freight Discounts: {0} {1}", rateDetail.TotalFreightDiscounts.Amount, rateDetail.TotalFreightDiscounts.Currency);
            //if (rateDetail.TotalSurcharges != null) Console.WriteLine("Total Surcharges: {0} {1}", rateDetail.TotalSurcharges.Amount, rateDetail.TotalSurcharges.Currency);
            //if (rateDetail.Surcharges != null)
            //{
            //    // Individual surcharge for each package
            //    foreach (Surcharge surcharge in rateDetail.Surcharges)
            //        Console.WriteLine(" {0} surcharge {1} {2}", surcharge.SurchargeType, surcharge.Amount.Amount, surcharge.Amount.Currency);
            if (rateDetail.TotalNetCharge != null)
            {
                if(rate < rateDetail.TotalNetCharge.Amount)
                {
                    rate = rateDetail.TotalNetCharge.Amount;
                }
            }
            else
            {
                return "*";
            }
            return rate.ToString("0.00");
        }
    }
}