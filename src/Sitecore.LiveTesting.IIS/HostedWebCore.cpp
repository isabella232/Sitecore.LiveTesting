#include <msclr\marshal_cppstd.h>

#include "HostedWebCore.h"

#pragma managed(push, off)

class CriticalSection
{
  private:
    CRITICAL_SECTION m_criticalSection;
    volatile bool m_disposed;

    CriticalSection(CriticalSection const&);
    void operator=(CriticalSection const&);
  public:
    CriticalSection();

    void Enter();
    void Leave();

    ~CriticalSection();
} instanceCriticalSection;

class CriticalSectionGuard
{
  private:
    CriticalSection& m_criticalSection;

    CriticalSectionGuard(CriticalSectionGuard const&);
    void operator=(CriticalSectionGuard const&);
  public:
    CriticalSectionGuard(CriticalSection& primitive);
    ~CriticalSectionGuard();
};

CriticalSection::CriticalSection()
{
  InitializeCriticalSection(&m_criticalSection);
  m_disposed = false;
}

void CriticalSection::Enter()
{
  if (!m_disposed)
  {
    EnterCriticalSection(&m_criticalSection);
  }
}

void CriticalSection::Leave()
{
  if (!m_disposed)
  {
    LeaveCriticalSection(&m_criticalSection);
  }
}

CriticalSection::~CriticalSection()
{
  m_disposed = true;
  DeleteCriticalSection(&m_criticalSection);
}

CriticalSectionGuard::CriticalSectionGuard(CriticalSection& primitive) : m_criticalSection(primitive)
{
  m_criticalSection.Enter();
}

CriticalSectionGuard::~CriticalSectionGuard()
{
  m_criticalSection.Leave();
}

#pragma managed(pop)

void Sitecore::LiveTesting::IIS::HostedWebCore::CreateHostedWebCore(_In_ HostedWebCoreSetup^ hostedWebCoreSetup)
{
  msclr::interop::marshal_context^ marshalContext = gcnew msclr::interop::marshal_context();
  
  try
  {
    CriticalSectionGuard instanceGuard(instanceCriticalSection);
    
    m_pHostedWebCore = new std::shared_ptr<NativeHostedWebCore>(NativeHostedWebCore::GetInstance(marshalContext->marshal_as<PCWSTR>(hostedWebCoreSetup->HostedWebCoreLibraryPath), marshalContext->marshal_as<PCWSTR>(hostedWebCoreSetup->HostConfig), marshalContext->marshal_as<PCWSTR>(hostedWebCoreSetup->RootConfig), marshalContext->marshal_as<PCWSTR>(hostedWebCoreSetup->InstanceName)));
  }
  catch (const std::runtime_error& e)
  {
    throw gcnew System::InvalidOperationException(gcnew System::String(e.what()));
  }
  catch (const std::invalid_argument& e)
  {
    throw gcnew System::ArgumentException(gcnew System::String(e.what()));
  }
}

Sitecore::LiveTesting::IIS::HostedWebCore::!HostedWebCore()
{
  this->~HostedWebCore();
}

Sitecore::LiveTesting::IIS::HostedWebCoreSetup^ Sitecore::LiveTesting::IIS::HostedWebCore::CurrentHostedWebCoreSetup::get()
{
  CriticalSectionGuard instanceGuard(instanceCriticalSection);

  return gcnew HostedWebCoreSetup(gcnew System::String(NativeHostedWebCore::GetCurrentHostedWebCoreLibraryPath().data()), gcnew System::String(NativeHostedWebCore::GetCurrentHostConfig().data()), gcnew System::String(NativeHostedWebCore::GetCurrentRootConfig().data()), gcnew System::String(NativeHostedWebCore::GetCurrentInstanceName().data()));
}

Sitecore::LiveTesting::IIS::HostedWebCore::HostedWebCore(_In_ HostedWebCoreSetup^ hostedWebCoreSetup)
{
  if (hostedWebCoreSetup == nullptr)
  {
    throw gcnew System::ArgumentNullException("hostedWebCoreSetup");
  }

  CreateHostedWebCore(hostedWebCoreSetup);
}

Sitecore::LiveTesting::IIS::HostedWebCore::HostedWebCore(_In_ System::String^ hostedWebCoreLibraryPath, _In_ System::String^ hostConfig, _In_ System::String^ rootConfig, _In_ System::String^ instanceName)
{
  CreateHostedWebCore(gcnew HostedWebCoreSetup(hostedWebCoreLibraryPath, hostConfig, rootConfig, instanceName));
}

Sitecore::LiveTesting::IIS::HostedWebCore::HostedWebCore(_In_ System::String^ hostConfig, _In_ System::String^ rootConfig, _In_ System::String^ instanceName)
{
  CreateHostedWebCore(gcnew HostedWebCoreSetup(System::IO::Path::Combine(System::Environment::GetFolderPath(System::Environment::SpecialFolder::ProgramFilesX86), "IIS Express\\hwebcore.dll"), hostConfig, rootConfig, instanceName));
}

Sitecore::LiveTesting::IIS::HostedWebCore::~HostedWebCore()
{
  CriticalSectionGuard instanceGuard(instanceCriticalSection);

  if (m_pHostedWebCore)
  {
    delete m_pHostedWebCore;
    m_pHostedWebCore = NULL;
  }
}