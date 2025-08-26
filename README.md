# DirectoryExplorer
CRUD application to navigate and perform actions on the user's filesystem

## Application architecture overview
This application utilizes a basic seperation between the api and the domain layer. The UI is built with JQuery, Ajax, and Materialize for quick CSS styling.

## Basic Setup
Before running, set the HomeDirectory property in the appsettings.json to a preferred directory on your machine.

## Needed Updates In Near Future
Now that the core functionality is built, the following are work items that should be tackled as the new high priority items:

* Unit Testing: Utilize Moq and XUnit to build a large library of unit tests with mocking
* Split code into additional logical groups
* Turn the UI into self contained components
* Containerize the application with Docker
* If additional functionality is needed, especially if persistence is ever required, the code would need to be further split into a domain layer and a repository layer. 

## Feature Opportunities For Expansion
The following are potential new features that can be added:

* Move & Rename: Support moving or renaming files/folders
* Preview: Quick preview for images, PDFs, and text files
* Search & Filter: Search by filename or extension; filter by type
* Sorting: Sort by name, size, or modified date
* Enhanced Upload: Multi-file upload, drag-and-drop, progress tracking
* User Management: Role-based read/write/delete permissions
* Versioning: Keep older versions of files
* Favorites: Bookmark frequently used folders
* Audit Logging: Track uploads, downloads, deletes
* Bulk Operations: Select and manage multiple items at once
* Multi User Management In Realtime: Allow multiple users to perform admin functions at the same time (think Google Docs/Sheets)