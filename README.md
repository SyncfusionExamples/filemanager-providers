# Syncfusion FileManager Providers

The React [File Manager](https://www.syncfusion.com/react-components/react-file-manager) is a component for managing files and folders in a web application. It provides a Windows Explorer-like interface for file operations such as viewing, selecting, uploading, downloading, sorting, filtering, creating, renaming, copying, moving, and deleting files and folders.

The service supports read, details, download, upload, create, delete, copy, move, rename, search, and image preview operations.

The File Manager is supported on multiple platforms including JavaScript, Angular, React, Vue, ASP.NET Core, ASP.NET MVC, TypeScript, and Blazor.

You can deploy the published image directly or create a custom Docker image based on the [Syncfusion File Manager providers repository](https://github.com/SyncfusionExamples/filemanager-providers). The unified image supports both Azure Blob Storage and Amazon S3. Select the provider with the `FILEMANAGER_PROVIDER` environment variable.

## Supported Providers

| Provider | `FILEMANAGER_PROVIDER` value | Storage settings |
| --- | --- | --- |
| Azure Blob Storage | `azure` | `AZURE_*` variables |
| Amazon S3 | `amazon-s3` | `AWS_*` variables |

Both providers support these File Manager operations:

| Operation | Description |
| --- | --- |
| Read | Lists files and folders. |
| Details | Returns file and folder details. |
| Download | Downloads a file or folder. |
| Upload | Uploads files, including chunked uploads. |
| Create | Creates a folder. |
| Delete | Deletes a file or folder. |
| Copy | Copies files or folders. |
| Move | Moves files or folders. |
| Rename | Renames a file or folder. |
| Search | Searches files and folders. |
| Image preview | Loads an image from storage. |

## Prerequisites

Before deploying the service, install [Docker](https://www.docker.com/products/container-runtime#/download).

### Windows

[Install Docker Desktop for Windows](https://docs.docker.com/desktop/setup/install/windows-install/)

### macOS

[Install Docker Desktop for macOS](https://docs.docker.com/desktop/setup/install/mac-install/)

Create either an Azure Blob Storage account or an Amazon S3 bucket before starting the service.

### Azure Blob Storage Requirements

Create and configure an Azure Storage Account and Blob Container for the File Manager service. You need:

- An active Microsoft Azure subscription.
- An Azure Storage Account.
- A Blob Container.
- The Storage Account Name.
- The Storage Account Access Key.
- The Blob Container Name.
- Permissions to perform file and folder operations.

See the [Azure Storage account creation guide](https://learn.microsoft.com/en-us/azure/storage/common/storage-account-create).

### Amazon S3 Requirements

Create and configure an Amazon S3 bucket for the File Manager service. You need:

- An active AWS account.
- An Amazon S3 bucket.
- An AWS Access Key ID.
- An AWS Secret Access Key.
- The AWS Region where the bucket is hosted.
- Permissions to perform file and folder operations.

See the [Amazon S3 bucket creation guide](https://docs.aws.amazon.com/AmazonS3/latest/userguide/create-bucket-overview.html).

## Configuration

> **Important:** `FILEMANAGER_PROVIDER` is required. Set it explicitly to `azure` or `amazon-s3`. If the variable is missing, empty, or contains another value, the service configuration is invalid.

### Azure Blob Storage

Set `FILEMANAGER_PROVIDER` to `azure`, then provide:

| Environment variable | Required | Description |
| --- | --- | --- |
| `AZURE_ACCOUNT_NAME` | Yes | Azure Storage account name. |
| `AZURE_ACCOUNT_KEY` | Yes | Azure Storage account key. |
| `AZURE_BLOB_NAME` | Yes | Blob container name. |
| `AZURE_BLOB_PATH` | Yes | Full URL of the blob container. Example: `https://<account>.blob.core.windows.net/<container>/` |
| `AZURE_FILE_PATH` | Yes | Full URL of the File Manager root path. Example: `https://<account>.blob.core.windows.net/<container>/<folder>` |

### Amazon S3

Set `FILEMANAGER_PROVIDER` to `amazon-s3`, then provide:

| Environment variable | Required | Description |
| --- | --- | --- |
| `AWS_ACCESS_KEY_ID` | Yes | AWS IAM access key ID. |
| `AWS_SECRET_ACCESS_KEY` | Yes | AWS IAM secret access key. |
| `AWS_BUCKET_NAME` | Yes | S3 bucket name. |
| `AWS_BUCKET_REGION` | Yes | AWS region containing the bucket. Example: `us-east-1` |

## Docker Deployment

**Step 1:** Pull the unified File Manager provider image from Docker Hub when using the published image:

```console
docker pull syncfusion/filemanager-provider:latest
```

**Step 2:** Create or update `docker-compose.yml` with the provider and matching storage credentials.

**Step 3:** Start the service:

```console
docker compose up
```

The service is available at:

```text
http://localhost:5000
```

Verify that the service is running:

```text
http://localhost:5000/api/Test
```

A successful response confirms that the service is ready to integrate with the File Manager component.

To stop it:

```console
docker compose down
```

The `docker-compose.yml` file contains the service configuration. Set `FILEMANAGER_PROVIDER` explicitly to `azure` or `amazon-s3` and provide only the matching storage settings. Use `docker compose up` without `--build` when using the published image directly.

For Amazon S3, the relevant Compose configuration is:

```yaml
version: '3.4' 

services:
  filemanager-provider:
    image: syncfusion/filemanager-provider:latest
    environment:
      FILEMANAGER_PROVIDER: amazon-s3
      AWS_ACCESS_KEY_ID: YOUR_AWS_ACCESS_KEY_ID
      AWS_SECRET_ACCESS_KEY: YOUR_AWS_SECRET_ACCESS_KEY
      AWS_BUCKET_NAME: YOUR_AWS_BUCKET_NAME
      AWS_BUCKET_REGION: us-east-1
    ports:
      - "5000:80"
```

For Azure Blob Storage, replace the provider and AWS settings with:

```yaml
environment:
  FILEMANAGER_PROVIDER: azure
  AZURE_ACCOUNT_NAME: YOUR_AZURE_ACCOUNT_NAME
  AZURE_ACCOUNT_KEY: YOUR_AZURE_ACCOUNT_KEY
  AZURE_BLOB_NAME: YOUR_AZURE_BLOB_NAME
  AZURE_BLOB_PATH: https://<account>.blob.core.windows.net/<container>/
  AZURE_FILE_PATH: https://<account>.blob.core.windows.net/<container>/<folder>
```

## File Manager AjaxSettings

The unified service uses the same endpoint names for both storage providers:

```javascript
const hostUrl = 'http://localhost:5000/';

const ajaxSettings = {
    url: hostUrl + 'api/FileManager/FileOperations',
    uploadUrl: hostUrl + 'api/FileManager/Upload',
    downloadUrl: hostUrl + 'api/FileManager/Download',
    getImageUrl: hostUrl + 'api/FileManager/GetImage'
};
```

The service also exposes a health endpoint at `GET /api/Test`.

## Supported Platforms

The File Manager component can be integrated with this service from multiple platforms:

| Platform | Getting Started |
|----------|----------------|
| JavaScript | https://help.syncfusion.com/file-manager-sdk/javascript/es5-getting-started |
| Angular | https://help.syncfusion.com/file-manager-sdk/angular/getting-started |
| React | https://help.syncfusion.com/file-manager-sdk/react/getting-started |
| Vue | https://help.syncfusion.com/file-manager-sdk/vue/getting-started |
| TypeScript | https://help.syncfusion.com/file-manager-sdk/typescript/getting-started |
| ASP.NET Core | https://help.syncfusion.com/file-manager-sdk/asp-net-core/getting-started |
| ASP.NET MVC | https://help.syncfusion.com/file-manager-sdk/asp-net-mvc/getting-started |
| Blazor | https://help.syncfusion.com/file-manager-sdk/blazor/getting-started-with-web-app |

For platform-specific configuration and integration details, refer to the corresponding getting started documentation.

## Support

Product support is available through the following mediums:

* [Contact Syncfusion support](https://www.syncfusion.com/support/directtrac/incidents) for expert help.
* Ask and connect with peers in the [Syncfusion community forum](https://www.syncfusion.com/forums/essential-js2).
* Create a [GitHub issue](https://github.com/syncfusion/ej2-javascript-ui-controls/issues/new).
* Ask questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/syncfusion) with the `syncfusion` and `ej2` tags.

## License

See the [Syncfusion license terms](https://github.com/syncfusion/ej2-javascript-ui-controls/blob/master/license).

## Changelog

See the [File Manager changelog](https://github.com/syncfusion/ej2-javascript-ui-controls/blob/master/controls/filemanager/CHANGELOG.md).

Copyright 2026 Syncfusion, Inc. All Rights Reserved. The Syncfusion Essential Studio license and copyright applies to this distribution.