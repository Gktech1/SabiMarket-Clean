using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Infrastructure.Data;

namespace SabiMarket.Infrastructure.Utilities
{
    public class IdGenerator
    {
        private readonly ApplicationDbContext _context;

        public IdGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateApprovalId(string companyName)
        {
            // Get the last inserted request where ApprovalId is not null
            var lastRequest = await _context.SowFoodStaffs
                .Where(r => r.Id != null)  // Filter records where ApprovalId is not null
                .OrderByDescending(r => r.Id)  // Sort by ApprovalId descending
                .FirstOrDefaultAsync();
            if (lastRequest is null)
            {
                return $"ID-0001";
            }
            int lastNumber = 0;

            // If a last request exists, extract the numeric part, else default to 0 (first entry scenario)
            if (lastRequest != null)
            {
                // Extract the numeric part from the ApprovalId after the last '/'
                var lastNumberString = lastRequest.StaffId.Substring(lastRequest.StaffId.LastIndexOf('-') + 1);
                int.TryParse(lastNumberString, out lastNumber);  // Parse the number part
            }

            // Increment the number for the new ID
            int newNumber = lastNumber + 1;

            // Generate the ApprovalId in the format ALTBANK/REQ/yyMMdd/0001
            var generatedId = $"SABIMARKET/REQ/{DateTime.UtcNow:yyMMdd}/{newNumber:D4}";

            return generatedId;
        }
    }
}
