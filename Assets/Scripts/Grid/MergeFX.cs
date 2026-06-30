using UnityEngine;

namespace PocketGarden.Grid
{
    /// <summary>
    /// One-shot particle burst played at a merge location. A short colored sparkle that fades out,
    /// tinted by the merged item's color. Spawned by <see cref="MergeGrid.TryMerge"/>.
    /// </summary>
    public static class MergeFX
    {
        private static Material _particleMat;

        public static void Spawn(Vector3 position, Color color)
        {
            var go = new GameObject("MergeFX");
            go.transform.position = position + Vector3.back * 0.5f;

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.duration = 0.6f;
            main.startLifetime = 0.7f;
            main.startSpeed = 2.6f;
            main.startSize = 0.14f;
            main.startColor = new ParticleSystem.MinMaxGradient(color, Color.white);
            main.gravityModifier = 1.2f;
            main.maxParticles = 30;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.25f;

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (_particleMat == null)
                _particleMat = new Material(Shader.Find("Particles/Standard Unlit")
                    ?? Shader.Find("Universal Render Pipeline/Particles/Unlit")
                    ?? Shader.Find("Sprites/Default"));
            renderer.material = _particleMat;
            renderer.sortingOrder = 40;

            ps.Play();
            Object.Destroy(go, 1.4f);
        }
    }
}
