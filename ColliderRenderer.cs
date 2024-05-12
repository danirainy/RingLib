// Credit to https://github.com/TheMulhima/HollowKnight.DebugMod

using GlobalEnums;
using System.Reflection;
using UnityEngine;

namespace RingLib;

internal class ColliderRenderer : MonoBehaviour
{
    public bool Enabled = false;

    private static class Drawing
    {
        private static Texture2D aaLineTex = null;
        private static Texture2D lineTex = null;
        private static Material blitMaterial = null;
        private static Material blendMaterial = null;
        private static Rect lineRect = new Rect(0, 0, 1, 1);
        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias)
        {
            float dx = pointB.x - pointA.x;
            float dy = pointB.y - pointA.y;
            float len = Mathf.Sqrt(dx * dx + dy * dy);
            if (len < 0.001f)
            {
                return;
            }
            Texture2D tex;
            Material mat;
            if (antiAlias)
            {
                width = width * 3.0f;
                tex = aaLineTex;
                mat = blendMaterial;
            }
            else
            {
                tex = lineTex;
                mat = blitMaterial;
            }
            float wdx = width * dy / len;
            float wdy = width * dx / len;
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.m00 = dx;
            matrix.m01 = -wdx;
            matrix.m03 = pointA.x + 0.5f * wdx;
            matrix.m10 = dy;
            matrix.m11 = wdy;
            matrix.m13 = pointA.y - 0.5f * wdy;
            GL.PushMatrix();
            GL.MultMatrix(matrix);
            Graphics.DrawTexture(lineRect, tex, lineRect, 0, 0, 0, 0, color, mat);
            if (antiAlias)
            {
                Graphics.DrawTexture(lineRect, tex, lineRect, 0, 0, 0, 0, color, mat);
            }
            GL.PopMatrix();
        }

        public static void DrawCircle(Vector2 center, int radius, Color color, float width, int segmentsPerQuarter)
        {
            DrawCircle(center, radius, color, width, false, segmentsPerQuarter);
        }

        public static void DrawCircle(Vector2 center, int radius, Color color, float width, bool antiAlias, int segmentsPerQuarter)
        {
            float rh = radius * 0.551915024494f;
            Vector2 p1 = new Vector2(center.x, center.y - radius);
            Vector2 p1_tan_a = new Vector2(center.x - rh, center.y - radius);
            Vector2 p1_tan_b = new Vector2(center.x + rh, center.y - radius);
            Vector2 p2 = new Vector2(center.x + radius, center.y);
            Vector2 p2_tan_a = new Vector2(center.x + radius, center.y - rh);
            Vector2 p2_tan_b = new Vector2(center.x + radius, center.y + rh);
            Vector2 p3 = new Vector2(center.x, center.y + radius);
            Vector2 p3_tan_a = new Vector2(center.x - rh, center.y + radius);
            Vector2 p3_tan_b = new Vector2(center.x + rh, center.y + radius);
            Vector2 p4 = new Vector2(center.x - radius, center.y);
            Vector2 p4_tan_a = new Vector2(center.x - radius, center.y - rh);
            Vector2 p4_tan_b = new Vector2(center.x - radius, center.y + rh);
            DrawBezierLine(p1, p1_tan_b, p2, p2_tan_a, color, width, antiAlias, segmentsPerQuarter);
            DrawBezierLine(p2, p2_tan_b, p3, p3_tan_b, color, width, antiAlias, segmentsPerQuarter);
            DrawBezierLine(p3, p3_tan_a, p4, p4_tan_b, color, width, antiAlias, segmentsPerQuarter);
            DrawBezierLine(p4, p4_tan_a, p1, p1_tan_a, color, width, antiAlias, segmentsPerQuarter);
        }

        public static void DrawBezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width,
                bool antiAlias, int segments)
        {
            Vector2 lastV = CubeBezier(start, startTangent, end, endTangent, 0);
            for (int i = 1; i < segments + 1; ++i)
            {
                Vector2 v = CubeBezier(start, startTangent, end, endTangent, i / (float)segments);
                DrawLine(lastV, v, color, width, antiAlias);
                lastV = v;
            }
        }

        private static Vector2 CubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
        {
            float rt = 1 - t;
            return rt * rt * rt * s + 3 * rt * rt * t * st + 3 * rt * t * t * et + t * t * t * e;
        }

        static Drawing()
        {
            Initialize();
        }

        private static void Initialize()
        {
            if (lineTex == null)
            {
                lineTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                lineTex.SetPixel(0, 1, Color.white);
                lineTex.Apply();
            }
            if (aaLineTex == null)
            {
                aaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, false);
                aaLineTex.SetPixel(0, 0, new Color(1, 1, 1, 0));
                aaLineTex.SetPixel(0, 1, Color.white);
                aaLineTex.SetPixel(0, 2, new Color(1, 1, 1, 0));
                aaLineTex.Apply();
            }
            blitMaterial = (Material)typeof(GUI).GetMethod("get_blitMaterial", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            blendMaterial = (Material)typeof(GUI).GetMethod("get_blendMaterial", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
        }
    }

    public struct HitboxType
    {
        public static readonly HitboxType Knight = new(Color.yellow, 0);                     // yellow
        public static readonly HitboxType Enemy = new(new Color(0.8f, 0, 0), 1);       // red      
        public static readonly HitboxType Attack = new(Color.cyan, 2);                       // cyan
        public static readonly HitboxType Terrain = new(new Color(0, 0.8f, 0), 3);     // green
        public static readonly HitboxType Trigger = new(new Color(0.5f, 0.5f, 1f), 4); // blue
        public static readonly HitboxType Breakable = new(new Color(1f, 0.75f, 0.8f), 5); // pink
        public static readonly HitboxType Gate = new(new Color(0.0f, 0.0f, 0.5f), 6); // dark blue
        public static readonly HitboxType HazardRespawn = new(new Color(0.5f, 0.0f, 0.5f), 7); // purple 
        public static readonly HitboxType Other = new(new Color(0.9f, 0.6f, 0.4f), 8); // orange
        public static readonly HitboxType None = new(new Color(0.9f, 0.6f, 0.4f), 9); // orange
        public readonly Color Color;
        public readonly int Depth;
        private HitboxType(Color color, int depth)
        {
            Color = color;
            Depth = depth;
        }
    }

    private Vector2 LocalToScreenPoint(Camera camera, Collider2D collider2D, Vector2 point)
    {
        Vector2 result = camera.WorldToScreenPoint((Vector2)collider2D.transform.TransformPoint(point + collider2D.offset));
        return new Vector2((int)Math.Round(result.x), (int)Math.Round(Screen.height - result.y));
    }

    public static HitboxType TryAddHitboxes(Collider2D collider2D)
    {
        if (collider2D == null)
        {
            return HitboxType.None;
        }
        if (collider2D is BoxCollider2D or PolygonCollider2D or EdgeCollider2D or CircleCollider2D)
        {
            GameObject go = collider2D.gameObject;
            if (collider2D.GetComponent<DamageHero>() || collider2D.gameObject.LocateMyFSM("damages_hero"))
            {
                return HitboxType.Enemy;
            }
            else if (go.GetComponent<HealthManager>() || go.LocateMyFSM("health_manager_enemy") || go.LocateMyFSM("health_manager"))
            {
                return HitboxType.Other;
            }
            else if (go.layer == (int)PhysLayers.TERRAIN)
            {
                if (go.name.Contains("Breakable") || go.name.Contains("Collapse") || go.GetComponent<Breakable>() != null) return HitboxType.Breakable;
                else return HitboxType.Terrain;
            }
            else if (go == HeroController.instance?.gameObject && !collider2D.isTrigger)
            {
                return HitboxType.Knight;
            }
            else if (go.GetComponent<DamageEnemies>() || go.LocateMyFSM("damages_enemy") || go.name == "Damager" && go.LocateMyFSM("Damage"))
            {
                return HitboxType.Attack;
            }
            else if (collider2D.isTrigger && collider2D.GetComponent<HazardRespawnTrigger>())
            {
                return HitboxType.HazardRespawn;
            }
            else if (collider2D.isTrigger && collider2D.GetComponent<TransitionPoint>())
            {
                return HitboxType.Gate;
            }
            else if (collider2D.GetComponent<Breakable>())
            {
                NonBouncer bounce = collider2D.GetComponent<NonBouncer>();
                if (bounce == null || !bounce.active)
                {
                    return HitboxType.Trigger;
                }
                return HitboxType.None;
            }
            else
            {
                return HitboxType.Other;
            }
        }
        return HitboxType.None;
    }

    private void OnGUI()
    {
        if (!Enabled)
        {
            return;
        }
        if (Event.current?.type != EventType.Repaint || Camera.main == null || GameManager.instance == null || GameManager.instance.isPaused)
        {
            return;
        }
        GUI.depth = int.MaxValue;
        Camera camera = Camera.main;
        float lineWidth = Math.Max(0.7f, Screen.width / 960f * GameCameras.instance.tk2dCam.ZoomFactor);
        foreach (var collider2D in gameObject.GetComponentsInChildren<Collider2D>())
        {
            var pairKey = TryAddHitboxes(collider2D);
            DrawHitbox(camera, collider2D, pairKey, lineWidth);
        }
    }

    private void DrawHitbox(Camera camera, Collider2D collider2D, HitboxType hitboxType, float lineWidth)
    {
        if (collider2D == null || !collider2D.isActiveAndEnabled)
        {
            return;
        }
        int origDepth = GUI.depth;
        GUI.depth = hitboxType.Depth;
        if (collider2D is BoxCollider2D or EdgeCollider2D or PolygonCollider2D)
        {
            switch (collider2D)
            {
                case BoxCollider2D boxCollider2D:
                    Vector2 halfSize = boxCollider2D.size / 2f;
                    Vector2 topLeft = new(-halfSize.x, halfSize.y);
                    Vector2 topRight = halfSize;
                    Vector2 bottomRight = new(halfSize.x, -halfSize.y);
                    Vector2 bottomLeft = -halfSize;
                    List<Vector2> boxPoints = new List<Vector2>
                        {
                            topLeft, topRight, bottomRight, bottomLeft, topLeft
                        };
                    DrawPointSequence(boxPoints, camera, collider2D, hitboxType, lineWidth);
                    break;
                case EdgeCollider2D edgeCollider2D:
                    DrawPointSequence(new(edgeCollider2D.points), camera, collider2D, hitboxType, lineWidth);
                    break;
                case PolygonCollider2D polygonCollider2D:
                    for (int i = 0; i < polygonCollider2D.pathCount; i++)
                    {
                        List<Vector2> polygonPoints = new(polygonCollider2D.GetPath(i));
                        if (polygonPoints.Count > 0)
                        {
                            polygonPoints.Add(polygonPoints[0]);
                        }
                        DrawPointSequence(polygonPoints, camera, collider2D, hitboxType, lineWidth);
                    }
                    break;
            }
        }
        else if (collider2D is CircleCollider2D circleCollider2D)
        {
            Vector2 center = LocalToScreenPoint(camera, collider2D, Vector2.zero);
            Vector2 right = LocalToScreenPoint(camera, collider2D, Vector2.right * circleCollider2D.radius);
            int radius = (int)Math.Round(Vector2.Distance(center, right));
            Drawing.DrawCircle(center, radius, hitboxType.Color, lineWidth, true, Mathf.Clamp(radius / 16, 4, 32));
        }
        GUI.depth = origDepth;
    }

    private void DrawPointSequence(List<Vector2> points, Camera camera, Collider2D collider2D, HitboxType hitboxType, float lineWidth)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 pointA = LocalToScreenPoint(camera, collider2D, points[i]);
            Vector2 pointB = LocalToScreenPoint(camera, collider2D, points[i + 1]);
            Drawing.DrawLine(pointA, pointB, hitboxType.Color, lineWidth, true);
        }
    }
}
