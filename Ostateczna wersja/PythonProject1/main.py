import pyodbc
import csv

# Database connection
connection_string = "DRIVER={ODBC Driver 17 for SQL Server};Server=LAPTOP-KGDC8LH1;Database=StronaZKursami;Trusted_Connection=yes;TrustServerCertificate=yes"

conn = pyodbc.connect(connection_string)
cursor = conn.cursor()

# Ensure the table exists
cursor.execute('''
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Quotes' AND xtype='U')
CREATE TABLE Quotes (
    QuoteId INT IDENTITY(1,1) PRIMARY KEY,
    Text NVARCHAR(1000),
    AuthorName NVARCHAR(255)
);
''')
conn.commit()


def import_quotes_from_file(filename):
    with open(filename, 'r', encoding='utf-8') as file:
        reader = csv.reader(file)

        # Skip the header row
        next(reader, None)

        for row in reader:
            if len(row) >= 3:
                try:
                    quote_text = row[1].strip()
                    author_name = row[2].strip()
                    cursor.execute(
                        "INSERT INTO Quotes (Text, AuthorName) VALUES (?, ?)",
                        quote_text, author_name
                    )
                except pyodbc.IntegrityError as e:
                    print(f"Integrity Error: {e}")
                except ValueError as e:
                    print(f"Invalid data format in row {row}: {e}")
            else:
                print(f"Skipped incomplete row: {row}")

    conn.commit()
    print("Import completed successfully.")
import_quotes_from_file('cytaty.txt')
cursor.close()
conn.close()
