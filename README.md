# BlobBuddy
Blob Buddy is a lightweight C# console application that provides a simple command-line interface for managing files in Azure Blob Storage. It supports uploading, downloading, listing, deleting, and generating SAS share links — all through a clean menu-driven interface.

Features
📤 Upload Files

Upload any local file to your Azure Blob container.

Automatically sets correct Content-Type headers based on file extension.

Uses the overwrite: true parameter for safe re-uploads.

📄 List Blobs

Displays blob names, sizes, and last modified timestamps in a readable format.

📥 Download Files

Download any blob to a chosen local folder (./downloads by default).

Automatically creates destination directories.

🗑️ Delete Blobs

Safely deletes files using DeleteIfExistsAsync().

🔗 Generate SAS Share Links

Create secure, time-limited SAS URLs (default: 24 hours).

Built using BlobSasBuilder and permission-scoped for read-only access.

⚡ Fully Asynchronous

All operations use async/await via the Azure SDK, improving responsiveness and scalability.

Tech Stack

C# / .NET

Azure Blob Storage

Azure.Storage.Blobs

Azure.Storage.Sas

Asynchronous programming (async/await)

Setup
1. Set Environment Variable

Blob Buddy uses the AZURE_STORAGE_CONNECTION_STRING environment variable for authentication.

export AZURE_STORAGE_CONNECTION_STRING="your_connection_string_here"

2. Restore & Build
dotnet restore
dotnet build

3. Run
dotnet run

Usage

Once running, the app shows an interactive menu:

=== blob buddy ===
1) upload
2) list
3) download
4) delete
5) share link (SAS)
0) exit


You can then choose an option and follow the prompts.

Project Structure
.
├── Program.cs      # Main application and menu loop
└── README.md       # Project documentation

Example SAS URL Output
sas url:
https://<account>.blob.core.windows.net/filesharecus/photo.jpg?sv=...

Future Improvements

Recursive folder uploads

Progress bars for large file transfers

Logging with Serilog

Windows/Linux installers

Optional encryption before upload
