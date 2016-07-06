using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using Acquaint.Service.DataObjects;
using Acquaint.Service.Models;
using System.Net.Http;
using System.Net;
using System.Web.Http.OData.Query;

namespace Acquaint.Service.Controllers
{
    public class AcquaintanceController : TableController<Acquaintance>
    {
        private AcquaintDbContext _context;

        const string DataPartitionIdPropertyName = "DataPartitionId";

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _context = new AcquaintDbContext();
            DomainManager = new EntityDomainManager<Acquaintance>(_context, Request);
        }

        // GET tables/Acquaintance/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public IQueryable<Acquaintance> GetAllAcquaintances(ODataQueryOptions<Acquaintance> queryOptions)
        {
            var DataPartitionId = GetDataPartitionId(queryOptions);

            // get acquaintances for DataPartitionId
            var acquaintances = Query().Where(x => x.DataPartitionId == DataPartitionId);

            // if there are no acquaintances for this DataPartitionId
            if (!acquaintances.Any())
            {
                // seed acquaintances for this DataPartitionId
                _context.Acquaintances.AddRange(GetSeedData(DataPartitionId));

                // commit context updates
                _context.SaveChanges();
            }

            // return acquaintances
            return Query().Where(x => x.DataPartitionId == DataPartitionId);
        }

        // GET tables/Acquaintance/3BF73C70-1DEE-47DC-8150-3A9BF368F01E
        public SingleResult<Acquaintance> GetAcquaintance(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Acquaintance/3BF73C70-1DEE-47DC-8150-3A9BF368F01E
        public async Task<Acquaintance> PatchAcquaintance(string id, Delta<Acquaintance> patch)
        {
            return await UpdateAsync(id, patch);
        }

        // POST tables/Acquaintance
        public async Task<IHttpActionResult> PostAcquaintance(Acquaintance item)
        {
            Acquaintance current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Acquaintance/3BF73C70-1DEE-47DC-8150-3A9BF368F01E
        public async Task DeleteAcquaintance(string id)
        {
            await DeleteAsync(id);
        }

        /// <summary>
        /// Parses the DataPartitionId from the OData query.
        /// </summary>
        /// <param name="queryOptions">The OData query as a ODataQueryOptions instance</param>
        /// <returns>The DataPartitionId string</returns>
        string GetDataPartitionId(ODataQueryOptions<Acquaintance> queryOptions)
        {
            if (queryOptions.Filter == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = $"Bad Request: OData $filter not present in request" });
            }

            if (!queryOptions.Filter.RawValue.Contains($"{DataPartitionIdPropertyName}"))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = $"Bad Request: {DataPartitionIdPropertyName} must be present in the OData $filter" });
            }

            var pattern = $"{DataPartitionIdPropertyName} eq '";

            var patternStart = queryOptions.Filter.RawValue.IndexOf(pattern);

            var value = queryOptions.Filter.RawValue.Substring(patternStart + pattern.Length, 36);

            Guid test;

            if (!Guid.TryParse(value, out test))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = $"Bad Request: {DataPartitionIdPropertyName} value must be a valid GUID" });
            }

            return value;
        }

        /// <summary>
        /// Generates Acwuaintances that will be associated with the given DataPartitionId
        /// </summary>
        /// <param name="dataPartitionId">A</param>
        /// <returns>A list of Acquaintances</returns>
        List<Acquaintance> GetSeedData(string dataPartitionId)
        {
            Guid test;

            if (!Guid.TryParse(dataPartitionId, out test))
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = $"Bad Request: {DataPartitionIdPropertyName} value must be a valid GUID" });
            }

            return new List<Acquaintance>()
                {
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Joseph", LastName = "Grimes", Company = "GG Mechanical", JobTitle = "Vice President", Email = "jgrimes@ggmechanical.com", Phone = "414-367-4348", Street = "2030 Judah St", City = "San Francisco", PostalCode = "94144", State = "CA", PhotoUrl = "josephgrimes.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Monica", LastName = "Green", Company = "Calcom Logistics", JobTitle = "Director", Email = "mgreen@calcomlogistics.com", Phone = "925-353-8029", Street = "230 3rd Ave", City = "San Francisco", PostalCode = "94118", State = "CA", PhotoUrl = "monicagreen.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Joan", LastName = "Mancum", Company = "Bay Unified School District", JobTitle = "Principal", Email = "joan.mancum@busd.org", Phone = "914-870-7670", Street = "448 Grand Ave", City = "South San Francisco", PostalCode = "94080", State = "CA", PhotoUrl = "joanmancum.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Alvin", LastName = "Gray", Company = "Pacific Cabinetry", JobTitle = "Office Manager", Email = "agray@pacificcabinets.com", Phone = "720-344-7823", Street = "1773 Lincoln St", City = "Santa Clara", PostalCode = "95050", State = "CA", PhotoUrl = "alvingray.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Michelle", LastName = "Wilson", Company = "Evergreen Mechanical", JobTitle = "Sales Manager", Email = "mwilson@evergreenmech.com", Phone = "917-245-7975", Street = "208 Jackson St", City = "San Jose", PostalCode = "95112", State = "CA", PhotoUrl = "michellewilson.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Jennifer", LastName = "Gillespie", Company = "Peninsula University", JobTitle = "Superintendent", Email = "jgillespie@peninsula.org", Phone = "831-427-6746", Street = "10002 N De Anza Blvd", City = "Cupertino", PostalCode = "95014", State = "CA", PhotoUrl = "jennifergillespie.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Thomas", LastName = "White", Company = "Creative Automotive Group", JobTitle = "Service Manager", Email = "tom.white@creativeauto.com", Phone = "214-865-0771", Street = "1181 Linda Mar Blvd", City = "Pacifica", PostalCode = "94044", State = "CA", PhotoUrl = "thomaswhite.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Leon", LastName = "Muks", Company = "Spacey", JobTitle = "President", Email = "leon.muks@spacey.io", Phone = "310-586-0181", Street = "2518 Durant Ave", City = "Berkeley", PostalCode = "94704", State = "CA", PhotoUrl = "leonmuks.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Floyd", LastName = "Bell", Company = "Netcore", JobTitle = "Procurement", Email = "floyd.bell@netcore.net", Phone = "603-226-4115", Street = "450 15th St", City = "Oakland", PostalCode = "94612", State = "CA", PhotoUrl = "floydbell.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Vanessa", LastName = "Thornton", Company = "Total Sources", JobTitle = "Product Manager", Email = "vanessa.thornton@totalsourcesinc.com", Phone = "419-998-6611", Street = "550 Quarry Rd", City = "San Carlos", PostalCode = "94070", State = "CA", PhotoUrl = "vanessathornton.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "John", LastName = "Boone", Company = "A. L. Price", JobTitle = "Executive Associate", Email = "jboone@alpricellc.com", Phone = "973-579-4610", Street = "233 E Harris Ave", City = "South San Francisco", PostalCode = "94080", State = "CA", PhotoUrl = "johnboone.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Ann", LastName = "Temple", Company = "Foxmoor", JobTitle = "Director", Email = "ann.temple@foxmoorinc.com", Phone = "608-821-7667", Street = "1270 San Pablo Ave", City = "Berkeley", PostalCode = "94706", State = "CA", PhotoUrl = "anntemple.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Joseph", LastName = "Meeks", Company = "Rose Records", JobTitle = "Manager", Email = "jmeeks@roserecordsllc.com", Phone = "978-628-6826", Street = "28 N 1st St", City = "San Jose", PostalCode = "95113", State = "CA", PhotoUrl = "josephmeeks.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Michelle", LastName = "Herring", Company = "Full Color", JobTitle = "Production Specialist", Email = "michelle.herring@fullcolorus.com", Phone = "201-319-9344", Street = "213 2nd Ave", City = "San Mateo", PostalCode = "94401", State = "CA", PhotoUrl = "michelleherring.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Daniel", LastName = "Jones", Company = "Flexus", JobTitle = "Quality Assurance Associate", Email = "daniel.jones@flexusinc.com", Phone = "228-432-8712", Street = "850 Bush St", City = "San Francisco", PostalCode = "94108", State = "CA", PhotoUrl = "danieljones.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Margaret", LastName = "Cargill", Company = "Redwood City Medical Group", JobTitle = "Director", Email = "mcargill@rcmg.org", Phone = "208-816-9793", Street = "1037 Middlefield Road", City = "Redwood City", PostalCode = "94063", State = "CA", PhotoUrl = "margaretcargill.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Benjamin", LastName = "Jones", Company = "JH Manufacturing", JobTitle = "Head of Manufacturing", Email = "ben.jones@jh.com", Phone = "505.562.3086", Street = "2091 Cowper St", City = "Palo Alto", PostalCode = "94306", State = "CA", PhotoUrl = "benjaminjones.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Ivan", LastName = "Diaz", Company = "XYZ Robotics", JobTitle = "CEO", Email = "ivan.diaz@xyzrobotics.com", Phone = "406-496-8774", Street = "1960 Mandela Parkway", City = "Oakland", PostalCode = "94607", State = "CA", PhotoUrl = "ivandiaz.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Eric", LastName = "Grant", Company = "MMSRI, Inc.", JobTitle = "Senior Manager", Email = "egrant@mmsri.com", Phone = "360-693-2388", Street = "2043 Martin Luther King Jr. Way", City = "Berkeley", PostalCode = "94704", State = "CA", PhotoUrl = "ericgrant.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Stacey", LastName = "Valdovinos", Company = "Global Manufacturing", JobTitle = "CEO", Email = "svaldovinos@globalmanuf.com", Phone = "440-243-7987", Street = "98 Udayakavi Lane", City = "Danville", PostalCode = "94526", State = "CA", PhotoUrl = "staceyvaldovinos.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Jesus", LastName = "Cardell", Company = "Pacific Marine Supply", JobTitle = "Manager", Email = "jcardella@pacificmarine.com", Phone = "410-745-5521", Street = "1008 Rachele Road", City = "Walnut Creek", PostalCode = "94597", State = "CA", PhotoUrl = "jesuscardell.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Wilma", LastName = "Woolley", Company = "Mission School District", JobTitle = "Superintendent", Email = "wwoolley@missionsd.org", Phone = "940-696-1852", Street = "7277 Moeser Lane", City = "El Cerrito", PostalCode = "94530", State = "CA", PhotoUrl = "wilmawoolley.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Evan", LastName = "Armstead", Company = "City of Richmond", JobTitle = "Board Member", Email = "evan.armstead@richmond.org", Phone = "415-336-2228", Street = "398 23rd St", City = "Richmond", PostalCode = "94804", State = "CA", PhotoUrl = "evanarmstead.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "James", LastName = "Jones", Company = "East Bay Commercial Bank", JobTitle = "Manager", Email = "james.jones@eastbaybank.com", Phone = "313-248-7644", Street = "4501 Pleasanton Way", City = "Pleasanton", PostalCode = "94556", State = "CA", PhotoUrl = "jamesjones.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Douglas", LastName = "Greenly", Company = "Bay Tech Credit Union", JobTitle = "Vice President", Email = "d.greenly@baytechcredit.com", Phone = "201-929-0094", Street = "2267 Alameda Ave", City = "Alameda", PostalCode = "94501", State = "CA", PhotoUrl = "douglasgreenly.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Brent", LastName = "Mason", Company = "Rockridge Hotel", JobTitle = "Concierge", Email = "brent.mason@rockridgehotel.com", Phone = "940-482-7759", Street = "1960 Mandela Parkway", City = "Oakland", PostalCode = "94607", State = "CA", PhotoUrl = "brentmason.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Richard", LastName = "Hogan", Company = "Marin Luxury Senior Living", JobTitle = "Customer Care", Email = "rhogan@marinseniorliving.com", Phone = "978-658-7545", Street = "674 Tiburon Blvd", City = "Belvedere Tiburon", PostalCode = "94920", State = "CA", PhotoUrl = "richardhogan.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Daniel", LastName = "Granville", Company = "Cityview Consulting", JobTitle = "Consultant", Email = "dgranville@cityviewconsulting.com", Phone = "330-616-7467", Street = "300 Spencer Ave", City = "Sausalito", PostalCode = "94965", State = "CA", PhotoUrl = "danielgranville.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Margaret", LastName = "Kidd", Company = "Marin Cultural Center", JobTitle = "President", Email = "mkidd@marincultural.org", Phone = "406-784-0602", Street = "106 Throckmorton Ave", City = "Mill Valley", PostalCode = "94941", State = "CA", PhotoUrl = "margaretkidd.jpg" },
                    new Acquaintance() { DataPartitionId = dataPartitionId, Id = Guid.NewGuid().ToString(), FirstName = "Leo", LastName = "Parson", Company = "San Rafel Chamber of Commerce", JobTitle = "Board Member", Email = "leo.parson@sanrafaelcoc.org", Phone = "773-991-5214", Street = "199 Clorinda Ave", City = "San Rafael", PostalCode = "94901", State = "CA", PhotoUrl = "leoparson.jpg" },
                };
        }
    }
}