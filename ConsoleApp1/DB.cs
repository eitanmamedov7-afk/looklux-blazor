
// SEARCH INDEX
// DATABASE, MYSQL, CONNECTION, BASE, QUERY
//
// Topic: LOW LEVEL DATABASE CONNECTION
// Purpose: Holds the shared MySQL connection setup used by every DB table class.
// Search keywords: DATABASE MYSQL CONNECTION BASE QUERY
// When to use it: Show this when explaining where SQL connections come from.
// Important notes: This file does not know about users/garments/outfits; table-specific logic is in child DB classes.

using MySql.Data.MySqlClient;
using System.Data.Common;

namespace DBL
{
    // SECTION: DATABASE CONNECTION BASE
    // Topic: DB base class
    // Purpose: Provides ADO.NET connection/command fields to BaseDB and table DB classes.
    // Search keywords: DATABASE MYSQL CONNECTION BASE
    // When to use it: Use when explaining inheritance in the DB layer.
    // Important notes: Do not put table-specific SQL here.
    // Lowest-level database base class.
    // Every DBL table class inherits from this, directly or through BaseDB<T>.
    // This affects all project database actions: users, garments, outfits, and outfit-garment links.
    public abstract class DB
    {

        // Connection string for the local MySQL server.
        // The schema queries usually use eitan_project12 explicitly, while this sets the default database.
        private const string MySqlConnSTR = @"server=localhost;
                                    user id=root;
                                    password=josh17rog;
                                    persistsecurityinfo=True;
                                    database=eitan_project12";

        // Shared ADO.NET objects used by BaseDB when it runs SQL commands.
        // Child DB classes do not create their own connection; they reuse this setup.
        protected DbConnection conn;
        protected DbCommand cmd;
        protected DbDataReader reader;

        // Creates the MySQL connection and command object for one DB helper instance.
        // No query is executed here; BaseDB opens/closes the connection around each action.
        protected DB()
        {
            if (conn == null)
            {
                conn = new MySqlConnection(MySqlConnSTR);
            }
            cmd = new MySqlCommand();
            cmd.Connection = conn;
            reader = null;
        }
    }
}
