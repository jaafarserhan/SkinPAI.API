using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkinPAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialAuthAndQuestionnaire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthProvider",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthProviderId",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "QuestionnaireCompleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "QuestionnaireCompletedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AmazonAffiliateUrl",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SephoraAffiliateUrl",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UltaAffiliateUrl",
                table: "Products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(6923));

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(7499));

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(7502));

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(7503));

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000005"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(7506));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(588));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(1127));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(1131));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(1134));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(4656), new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(4657) });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(5425), new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(5426) });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(5437), new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(5437) });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(5440), new DateTime(2026, 2, 21, 14, 32, 2, 966, DateTimeKind.Utc).AddTicks(5440) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthProvider",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AuthProviderId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "QuestionnaireCompleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "QuestionnaireCompletedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AmazonAffiliateUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SephoraAffiliateUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UltaAffiliateUrl",
                table: "Products");

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(4482));

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(5002));

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(5004));

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(5006));

            migrationBuilder.UpdateData(
                table: "Achievements",
                keyColumn: "AchievementId",
                keyValue: new Guid("a0000000-0000-0000-0000-000000000005"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(5007));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 13, 49, 18, 797, DateTimeKind.Utc).AddTicks(8571));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 13, 49, 18, 797, DateTimeKind.Utc).AddTicks(9075));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 13, 49, 18, 797, DateTimeKind.Utc).AddTicks(9079));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "CreatedAt",
                value: new DateTime(2026, 2, 21, 13, 49, 18, 797, DateTimeKind.Utc).AddTicks(9080));

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(2348), new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(2349) });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(3063), new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(3063) });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(3066), new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(3066) });

            migrationBuilder.UpdateData(
                table: "SubscriptionPlans",
                keyColumn: "PlanId",
                keyValue: new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(3068), new DateTime(2026, 2, 21, 13, 49, 18, 798, DateTimeKind.Utc).AddTicks(3068) });
        }
    }
}
