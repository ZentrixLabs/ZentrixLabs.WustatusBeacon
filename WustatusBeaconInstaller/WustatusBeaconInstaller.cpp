#include <windows.h>
#include <string>
#include <iostream>

#define SERVICE_NAME L"Wustatus.Beacon"
#define DISPLAY_NAME L"ZentrixLabs Windows Update Status Beacon"
#define SERVICE_DESCRIPTION L"Service to create a local endpoint for Windows Update Status Reporting"
#define EVENT_SOURCE L"WustatusBeaconInstaller"

// Event ID definitions
#define EVENT_INSTALL_SUCCESS 1000
#define EVENT_INSTALL_FAILURE 1001
#define EVENT_START_FAILURE   1002
#define EVENT_UNINSTALL_SUCCESS 2000
#define EVENT_UNINSTALL_FAILURE 2001

void LogEvent(WORD type, DWORD eventID, const std::wstring& message) {
    HANDLE hEventLog = RegisterEventSourceW(nullptr, EVENT_SOURCE);
    if (hEventLog) {
        LPCWSTR strings[1] = { message.c_str() };
        ReportEventW(hEventLog, type, 0, eventID, nullptr, 1, 0, strings, nullptr);
        DeregisterEventSource(hEventLog);
    }
}

// Function to install the service
bool InstallService(const std::wstring& serviceBinaryPath) {
    const std::wstring installDir = L"C:\\Program Files\\ZentrixLabs\\WustatusBeacon";
    const std::wstring finalBinaryPath = installDir + L"\\WustatusBeacon.exe";

    // Create install directory if it doesn't exist
    CreateDirectoryW(installDir.c_str(), nullptr);

    // Copy the service binary to install location
    if (!CopyFileW(serviceBinaryPath.c_str(), finalBinaryPath.c_str(), FALSE)) {
        std::wcerr << L"Failed to copy binary to install path: " << GetLastError() << std::endl;
        LogEvent(EVENTLOG_ERROR_TYPE, EVENT_INSTALL_FAILURE, L"CopyFile failed during install.");
        return false;
    }

    SC_HANDLE schSCManager = OpenSCManager(nullptr, nullptr, SC_MANAGER_ALL_ACCESS);
    if (schSCManager == nullptr) {
        std::wcerr << L"OpenSCManager failed: " << GetLastError() << std::endl;
        LogEvent(EVENTLOG_ERROR_TYPE, EVENT_INSTALL_FAILURE, L"OpenSCManager failed.");
        return false;
    }

    SC_HANDLE schService = CreateServiceW(
        schSCManager,
        SERVICE_NAME,
        DISPLAY_NAME,
        SERVICE_ALL_ACCESS,
        SERVICE_WIN32_OWN_PROCESS,
        SERVICE_AUTO_START,
        SERVICE_ERROR_NORMAL,
        finalBinaryPath.c_str(),  // 👈 install path here
        nullptr, nullptr, nullptr, nullptr, nullptr
    );

    if (schService == nullptr) {
        std::wcerr << L"CreateService failed: " << GetLastError() << std::endl;
        LogEvent(EVENTLOG_ERROR_TYPE, EVENT_INSTALL_FAILURE, L"CreateService failed.");
        CloseServiceHandle(schSCManager);
        return false;
    }

    SERVICE_DESCRIPTIONW description = { 0 };
    description.lpDescription = (LPWSTR)SERVICE_DESCRIPTION;
    ChangeServiceConfig2W(schService, SERVICE_CONFIG_DESCRIPTION, &description);

    std::wcout << L"Service installed successfully." << std::endl;
    LogEvent(EVENTLOG_INFORMATION_TYPE, EVENT_INSTALL_SUCCESS, L"Service installed successfully.");

    if (!StartServiceW(schService, 0, nullptr)) {
        std::wcerr << L"StartService failed: " << GetLastError() << std::endl;
        LogEvent(EVENTLOG_WARNING_TYPE, EVENT_START_FAILURE, L"Service installed but failed to start.");
    }
    else {
        std::wcout << L"Service started successfully." << std::endl;
    }

    CloseServiceHandle(schService);
    CloseServiceHandle(schSCManager);
    return true;
}

// Function to uninstall the service
bool UninstallService() {
    const std::wstring installDir = L"C:\\Program Files\\ZentrixLabs\\WustatusBeacon";
    const std::wstring finalBinaryPath = installDir + L"\\WustatusBeacon.exe";

    SC_HANDLE schSCManager = OpenSCManager(nullptr, nullptr, SC_MANAGER_ALL_ACCESS);
    if (schSCManager == nullptr) {
        std::wcerr << L"OpenSCManager failed: " << GetLastError() << std::endl;
        LogEvent(EVENTLOG_ERROR_TYPE, EVENT_UNINSTALL_FAILURE, L"OpenSCManager failed during uninstall.");
        return false;
    }

    SC_HANDLE schService = OpenServiceW(schSCManager, SERVICE_NAME, SERVICE_ALL_ACCESS);
    if (schService == nullptr) {
        std::wcerr << L"OpenService failed: " << GetLastError() << std::endl;
        LogEvent(EVENTLOG_ERROR_TYPE, EVENT_UNINSTALL_FAILURE, L"OpenService failed during uninstall.");
        CloseServiceHandle(schSCManager);
        return false;
    }

    SERVICE_STATUS status;
    if (ControlService(schService, SERVICE_CONTROL_STOP, &status)) {
        std::wcout << L"Stopping service..." << std::endl;
        while (QueryServiceStatus(schService, &status) && status.dwCurrentState != SERVICE_STOPPED) {
            Sleep(500);
        }
        std::wcout << L"Service stopped." << std::endl;
    }

    if (!DeleteService(schService)) {
        std::wcerr << L"DeleteService failed: " << GetLastError() << std::endl;
        LogEvent(EVENTLOG_ERROR_TYPE, EVENT_UNINSTALL_FAILURE, L"DeleteService failed.");
        CloseServiceHandle(schService);
        CloseServiceHandle(schSCManager);
        return false;
    }

    CloseServiceHandle(schService);
    CloseServiceHandle(schSCManager);

    // Delete service binary
    if (!DeleteFileW(finalBinaryPath.c_str())) {
        std::wcerr << L"Failed to delete binary: " << GetLastError() << std::endl;
    }

    // Delete install directory (will fail if anything remains inside)
    if (!RemoveDirectoryW(installDir.c_str())) {
        std::wcerr << L"Failed to remove install directory (might not be empty): " << GetLastError() << std::endl;
    }

    std::wcout << L"Service uninstalled and cleaned up successfully." << std::endl;
    LogEvent(EVENTLOG_INFORMATION_TYPE, EVENT_UNINSTALL_SUCCESS, L"Service uninstalled and cleaned up successfully.");
    return true;
}

int wmain(int argc, wchar_t* argv[]) {
    if (argc < 2) {
        std::wcerr << L"Usage: ZentrixLabs.WustatusBeaconInstaller.exe [--install <path_to_binary> | --uninstall]" << std::endl;
        return 1;
    }

    std::wstring command = argv[1];

    if (command == L"--install") {
        if (argc != 3) {
            std::wcerr << L"Usage for install: ZentrixLabs.WustatusBeaconInstaller.exe --install <path_to_binary>" << std::endl;
            return 1;
        }

        std::wstring binaryPath = argv[2];
        if (binaryPath.find(L" ") != std::wstring::npos && binaryPath[0] != L'"') {
            binaryPath = L"\"" + binaryPath + L"\"";
        }

        if (!InstallService(binaryPath)) return 1;
    }
    else if (command == L"--uninstall") {
        if (!UninstallService()) return 1;
    }
    else {
        std::wcerr << L"Invalid command. Use --install or --uninstall." << std::endl;
        return 1;
    }

    return 0;
}
