namespace Merchello.Core.Gateways.Shipping.FedEx
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Models;
    using Models.Interfaces;
    using Umbraco.Core;
    using Configuration;

    /// <summary>
    /// Defines the rate table ship method
    /// </summary>
    [GatewayMethodEditor("FedEx ship method editor", "Fixed rate ship method editor", "~/App_Plugins/Merchello/Backoffice/Merchello/Dialogs/shipping.FedEx.shipmethod.html")]
    public class FedExShippingGatewayMethod : ShippingGatewayMethodBase, IFedExShippingGatewayMethod
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FedExShippingGatewayMethod"/> class.
        /// </summary>
        /// <param name="gatewayResource">
        /// The gateway resource.
        /// </param>
        /// <param name="shipMethod">
        /// The ship method.
        /// </param>
        /// <param name="shipCountry">
        /// The ship country.
        /// </param>
        public FedExShippingGatewayMethod(IGatewayResource gatewayResource, IShipMethod shipMethod, IShipCountry shipCountry)
            : base(gatewayResource, shipMethod, shipCountry)
        {

        }

        /// <summary>
        /// The quote shipment.
        /// </summary>
        /// <param name="shipment">
        /// The shipment.
        /// </param>
        /// <returns>
        /// The <see cref="Attempt"/>.
        /// </returns>
        public override Attempt<IShipmentRateQuote> QuoteShipment(IShipment shipment)
        {
            // TODO this should be made configurable
            var visitor = new FedExShipmentLineItemVisitor { UseOnSalePriceIfOnSale = MerchelloConfiguration.Current.QuoteShipmentUsingOnSalePrice };

            shipment.Items.Accept(visitor);

            var province = ShipMethod.Provinces.FirstOrDefault(x => x.Code == shipment.ToRegion);

            var rateList = RateAvailableServiceWebServiceClient.FedexRate.Rates(shipment.ToAddress1, shipment.ToLocality, shipment.ToRegion, shipment.ToPostalCode, shipment.ToCountryCode, visitor.TotalWeight);

            if (rateList.Contains(ShipMethod.Name))
            {
                return Attempt<IShipmentRateQuote>.Succeed(new ShipmentRateQuote(shipment, ShipMethod) { Rate = (System.Convert.ToDecimal(rateList.ElementAt(rateList.FindIndex(x => x.Contains(ShipMethod.Name)) + 1)) * 1.23M)});
            }
            else
            {
                return Attempt<IShipmentRateQuote>.Fail(new IndexOutOfRangeException("The shipment type is not supported for this address."));
            }
        }
    }
}