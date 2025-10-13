# Running MyApp in GitHub Codespaces

This guide will walk you through setting up and running this .NET Aspire application in GitHub Codespaces.

## Prerequisites

- A GitHub account
- Access to this repository

## Quick Start Guide

### Step 1: Create a Codespace

1. Navigate to the repository on GitHub: `https://github.com/bancroftway/ideal-garbanzo`
2. Click the green **Code** button
3. Select the **Codespaces** tab
4. Click **Create codespace on feature/devcontainer-codespaces** (or your current branch)
5. **Important**: Select the **4-core • 16 GB RAM • 32 GB storage** machine type (best free tier option)
   - You get 120 core-hours/month free (30 hours on 4-core machine)
   - This is recommended due to the multiple containers running

### Step 2: Wait for Initialization

The Codespace will automatically:
- ✅ Build the container using .NET 10 nightly SDK
- ✅ Install Docker-in-Docker feature
- ✅ Install GitHub CLI (`gh`)
- ✅ Install Azure CLI (`az`)
- ✅ Install Aspire CLI (`dotnet tool install -g Aspire.Cli --prerelease`)
- ✅ Restore NuGet packages for the solution
- ⏱️ This takes approximately 2-5 minutes on first launch

### Step 3: Configure Secrets (Optional)

The application uses default credentials for RabbitMQ:
- **Username**: `guest`
- **Password**: `password1234!!`

If you want to use custom credentials via Codespaces secrets:

1. Go to repository **Settings** → **Secrets and variables** → **Codespaces**
2. Add the following secrets:
   - `USERNAME` → your desired username
   - `PASSWORD` → your desired password
3. Restart the Codespace or reload the window

**Note**: For development purposes, the defaults work fine!

### Step 4: Build the Solution

Once initialization completes, build the solution:

```bash
cd src
dotnet build MyApp.slnx
```

Expected output: `Build succeeded`

### Step 5: Run the Aspire AppHost

Start the application using one of these methods:

#### Method A: Using VS Code Debug (Recommended)
1. Press `F5` or click **Run and Debug** in the sidebar
2. Select **Launch Aspire AppHost**
3. The Aspire Dashboard will open automatically

#### Method B: Using Terminal
```bash
cd src/MyApp.AppHost
dotnet run
```

#### Method C: Using VS Code Task
1. Press `Ctrl+Shift+P` (or `Cmd+Shift+P` on Mac)
2. Type "Tasks: Run Task"
3. Select "Run Aspire AppHost"

### Step 6: Access the Aspire Dashboard

1. VS Code will show a notification: **"Your application is running on port 15888"**
2. Click **Open in Browser** or navigate to the **PORTS** tab
3. Find port **15888** (Aspire Dashboard HTTP) and click the globe icon
4. The Aspire Dashboard will open in a new browser tab

**What you'll see**:
- 📊 Dashboard showing all services (MyApp, PostgreSQL, RabbitMQ, Docling, Qdrant)
- 🔄 Container status (Starting → Running)
- 📝 Logs for each service
- 🌐 Endpoint URLs

### Step 7: Access Your Application

From the Aspire Dashboard:
1. Find the **MyApp** service
2. Click the endpoint URL (usually shows as `http://myapp:5000`)
3. Your Blazor application will open in a new tab

Alternatively, use the PORTS tab in VS Code:
- Port **5000** → MyApp HTTP
- Port **5001** → MyApp HTTPS

## Service Endpoints

All services are accessible via port forwarding:

| Service | Port | Description | Access |
|---------|------|-------------|--------|
| Aspire Dashboard | 15888 | Main orchestration dashboard | Auto-forwarded |
| MyApp | 5000/5001 | Your Blazor application | Via dashboard or ports tab |
| PostgreSQL | 5432 | Database server | Internal |
| PgAdmin | (varies) | PostgreSQL web UI | Check dashboard |
| RabbitMQ | 5672 | Message broker | Internal |
| RabbitMQ Management | 15672 | RabbitMQ admin UI | Via ports tab |
| Qdrant | 6333 | Vector database HTTP | Internal |
| Qdrant | 6334 | Vector database gRPC | Internal |
| Docling | 8001 | Document processing API | Via ports tab |

## Important Notes

### 🔄 Data Persistence
- **Container data is ephemeral** - it will be lost when the Codespace stops
- Data volumes are recreated each time the Codespace starts
- This is expected behavior for development environments

### 🐳 First-Time Container Pulls
The first time you run the application:
- Docker will pull all container images (PostgreSQL, RabbitMQ, Qdrant, Docling)
- Docling will download AI models (~900MB)
- **This can take 5-15 minutes** depending on connection speed
- Subsequent runs will be much faster (containers are cached)

### ⚡ Performance Tips
- Use the 4-core machine type for best performance
- Stop the Codespace when not in use (doesn't count against free hours)
- Containers are cached, so restarts after the first time are fast

### 🔧 Troubleshooting

**Problem**: "Unable to connect to Docker"
**Solution**: Wait 30-60 seconds for Docker daemon to fully start, then retry

**Problem**: Aspire CLI not found
**Solution**: Run `export PATH="$PATH:$HOME/.dotnet/tools"` and retry

**Problem**: Ports not forwarding
**Solution**: Check the PORTS tab and manually forward port 15888 if needed

**Problem**: Out of memory errors
**Solution**: Reduce the number of containers or upgrade to a larger machine type

**Problem**: Docling takes forever to start
**Solution**: First run downloads models. Check Aspire Dashboard logs for progress

## Development Workflow

### Making Code Changes

1. Edit files in VS Code as normal
2. The application supports hot reload for most changes
3. For major changes, stop (Ctrl+C) and restart the AppHost

### Viewing Logs

Use the Aspire Dashboard:
1. Navigate to the dashboard (port 15888)
2. Click on any service
3. View real-time logs, metrics, and traces

### Debugging

1. Set breakpoints in your code
2. Press `F5` to start debugging
3. Debugging works for the MyApp project
4. Container services (PostgreSQL, etc.) show logs in the dashboard

### Stopping Services

- **Stop AppHost**: Press `Ctrl+C` in the terminal
- **Stop Codespace**: Click the Codespaces menu (bottom-left) → **Stop Current Codespace**
- Containers will automatically stop when AppHost stops

## Working with Services

### Accessing PostgreSQL

**Via PgAdmin**:
- Check the Aspire Dashboard for the PgAdmin endpoint
- Use credentials from the dashboard

**Via Command Line**:
```bash
# Install psql if needed
apt-get update && apt-get install -y postgresql-client

# Connect (replace port with actual exposed port)
psql -h localhost -p 5432 -U postgres -d myappdb
```

### Accessing RabbitMQ Management

1. Open the PORTS tab in VS Code
2. Find port **15672** (RabbitMQ Management)
3. Click the globe icon to open in browser
4. Login with: `guest` / `password1234!!`

### Accessing Qdrant

- Dashboard available in Aspire Dashboard
- HTTP API on port 6333
- Use Qdrant client libraries or REST API

## Advanced Configuration

### Customizing Container Lifetime

Edit `src/MyApp.AppHost/AppHost.cs` to change container behavior:
- `ContainerLifetime.Persistent` → keeps data between runs (but not between Codespace sessions)
- `ContainerLifetime.Session` → destroys data on stop

### Environment Variables

Add to `.devcontainer/devcontainer.json` under `containerEnv`:
```json
"containerEnv": {
  "MY_CUSTOM_VAR": "value"
}
```

### Installing Additional Tools

Edit `.devcontainer/devcontainer.json` and add to `postCreateCommand`:
```json
"postCreateCommand": "dotnet tool install -g Aspire.Cli --prerelease && dotnet restore src/MyApp.slnx && apt-get update && apt-get install -y <your-tool>"
```

## Cost Considerations

GitHub Codespaces free tier includes:
- **120 core-hours/month** (for individual accounts)
- **15 GB storage**

With a 4-core machine:
- **30 hours of usage per month** (120 ÷ 4 = 30)
- Stopped Codespaces don't count against hours
- Storage persists even when stopped (counts against 15GB limit)

**Best practices**:
- Stop Codespaces when not actively developing
- Delete unused Codespaces to free storage
- Consider prebuild configuration for faster startup

## Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [GitHub Codespaces Documentation](https://docs.github.com/en/codespaces)
- [Dev Container Specification](https://containers.dev/)

## Support

If you encounter issues:
1. Check the Aspire Dashboard logs
2. Check the PORTS tab for port forwarding status
3. Review the terminal output for errors
4. Open an issue in this repository

---

**Happy coding in Codespaces! 🚀**
