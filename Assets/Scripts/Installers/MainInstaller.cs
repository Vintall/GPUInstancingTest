using Services.NoiseGeneration.Impls;
using UnityEngine;
using Zenject;

namespace Services.Installers
{
    public class MainInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            //InstallServices();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void InstallServices()
        {
            //Debug.Log($"InstallServices");
            //Container.BindInterfacesTo<PerlinNoiseGenerator>().AsSingle().NonLazy();
            //Container.BindInterfacesTo<PlaneGenerator>();
        }
    }
}