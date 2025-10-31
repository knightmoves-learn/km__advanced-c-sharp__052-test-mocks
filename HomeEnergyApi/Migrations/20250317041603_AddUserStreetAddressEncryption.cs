﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeEnergyApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStreetAddressEncryption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptedHomeStreetAddress",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedHomeStreetAddress",
                table: "Users");
        }
    }
}
