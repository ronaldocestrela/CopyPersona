using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonaScript.Modules.Billing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_TenantId_Status",
                schema: "billing",
                table: "Subscriptions",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UsageQuotas_TenantId_PeriodEnd",
                schema: "billing",
                table: "UsageQuotas",
                columns: new[] { "TenantId", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_QuotaTransactions_TenantId_TransactionDate",
                schema: "billing",
                table: "QuotaTransactions",
                columns: new[] { "TenantId", "TransactionDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_TenantId_Status",
                schema: "billing",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_UsageQuotas_TenantId_PeriodEnd",
                schema: "billing",
                table: "UsageQuotas");

            migrationBuilder.DropIndex(
                name: "IX_QuotaTransactions_TenantId_TransactionDate",
                schema: "billing",
                table: "QuotaTransactions");
        }
    }
}
