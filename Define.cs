using SXH.Character;
using SXH.Scene;
using System.Collections.Generic;
using UnityEngine;

namespace Hunter
{
    public interface IShapeParam
    {
        Vector3 Center { get; }
        bool Inside(Vector3 point);

        bool IsIntersect(RectParam rect);
    }

    public static class ShapeParam
    {
        public static IShapeParam Get(int type, List<float> param, ISceneUnit unit)
        {
            IShapeParam shape = null;
            switch (type)
            {
                case 1:
                    shape = new CircleParam(param, unit);
                    break;
                case 2:
                    RectParam rect = new RectParam();
                    rect.Init(param, unit);
                    shape = rect;
                    break;
                case 3:
                    shape = new SectorParam(param, unit);
                    break;
                case 4:
                    shape = new SphereParam(param, unit);
                    break;
            }
            return shape;
        }

        public static IShapeParam Get(AttackCheckParam data, ISceneUnit unit)
        {
            return Get(data.IAreaType, data.fListShapeParam, unit);
        }

        public static RectParam GetRect(ISceneUnit unit)
        {
            var coll = unit as IBodyCollider;
            RectParam rect = new RectParam();
            if (coll == null)
            {
                return rect;
            }
            rect.Init(coll.ColliderCenter, coll.ColliderSize, unit);
            return rect;
        }

        public static bool Check(AttackCheckParam data, ISceneUnit unit, RectParam rect, ref Vector3 hit_center)
        {
            return Check(data.IAreaType, data.fListShapeParam, unit, rect, ref hit_center);
        }

        public static bool Check(int type, List<float> param, ISceneUnit unit, RectParam rect, ref Vector3 hit_center)
        {
            bool result = false;
            switch (type)
            {
                case 1:
                {
                    CircleParam shape = new CircleParam(param, unit);
                    hit_center = shape.Center;
                    result = shape.IsIntersect(rect);
                }
                break;
                case 2:
                {
                    RectParam shape = new RectParam();
                    shape.Init(param, unit);
                    hit_center = shape.Center;
                    result = shape.IsIntersect(rect);
                }
                break;
                case 3:
                {
                    SectorParam shape = new SectorParam(param, unit);
                    hit_center = shape.Center;
                    result = shape.IsIntersect(rect);
                }
                break;
                case 4:
                {
                    SphereParam shape = new SphereParam(param, unit);
                    hit_center = shape.Center;
                    result = shape.IsIntersect(rect);
                }
                break;
            }
            return result;
        }
    }

    public struct PointParam
    {
        public Vector3 Position { get; private set; }
        public float Height { get; private set; }

        public PointParam(Vector3 pos, float height)
        {
            Position = pos + Vector3.up * height * 0.5f;
            Height = height;
        }

        public PointParam(ISceneUnit unit)
        {
            Height = 1;
            if (unit is IBodyCollider unit2)
            {
                Height = unit2.ColliderSize.y;
            }
            Position = Utility.Unit.GetPosition(unit) + Vector3.up * Height * 0.5f;
        }
    }

    public struct CircleParam : IShapeParam
    {
        public Vector3 Center { get; private set; }
        public float Radius { get; private set; }
        public float Height { get; private set; }

        public CircleParam(List<float> shape_param, ISceneUnit unit)
        {
            Radius = shape_param.Count > 0 ? shape_param[0] : 0;
            Height = shape_param.Count > 1 ? shape_param[1] : 0;

            Center = Utility.Unit.GetPosition(unit);
        }

        public bool Inside(Vector3 point)
        {
            var dir = point - Center;
            dir.y = 0;

            if (dir.magnitude > Radius)
            {
                return false;
            }

            return true;
        }

        public bool IsIntersect(RectParam rect)
        {
            if ((Center.y + Height * 0.5f) < (rect.Center.y - rect.Size.y * 0.5f) || (rect.Center.y + rect.Size.y * 0.5f) < (Center.y - Height * 0.5f))
            {
                return false;
            }
            return Utility.Math.IsIntersectRectWithCircle(rect.PlaneCorner, Center, Radius);
        }
    }

    public struct RectParam : IShapeParam
    {
        public Vector3 Center { get; private set; }

        public Vector3 Size { get; private set; }

        public Vector3[] PlaneCorner { get; private set; }

        public Vector3 NHForward { get; private set; }
        public Vector3 NHRight { get; private set; }



        public void Init(List<float> shape_param, ISceneUnit unit)
        {
            var forward = Utility.Unit.GetDirection(unit);
            var right = Utility.Unit.GetRightDir(unit);

            var z = shape_param.Count > 0 ? shape_param[0] : 0;
            var x = shape_param.Count > 1 ? shape_param[1] : 0;
            var y = shape_param.Count > 2 ? shape_param[2] : 0;

            Size = new Vector3(x, y, z);

            //Center = UnitUtils.GetPosition(unit) + unit.transform.TransformVector(
            //    new Vector3(arr_offset[0], arr_offset[1], arr_offset[2]));

            Center = Utility.Unit.GetPosition(unit);

            forward.y = 0;
            right.y = 0;

            NHForward = forward.normalized;
            NHRight = right.normalized;

            PlaneCorner = new Vector3[4];
            PlaneCorner[0] = Center - Size.x * NHRight * 0.5f;
            PlaneCorner[1] = Center + Size.x * NHRight * 0.5f;
            PlaneCorner[2] = PlaneCorner[1] + Size.z * NHForward;
            PlaneCorner[3] = PlaneCorner[0] + Size.z * NHForward;
        }

        public void Init(Vector3 center, Vector3 size, ISceneUnit unit)
        {
            Center = center;
            Size = size;

            var forward = Utility.Unit.GetDirection(unit);
            var right = Utility.Unit.GetRightDir(unit);

            NHForward = forward.normalized;
            NHRight = right.normalized;

            var dis_x = 0.5f * size.x * NHRight;
            var dis_z = 0.5f * size.z * NHForward;

            PlaneCorner = new Vector3[4];
            PlaneCorner[0] = Center - dis_x - dis_z;
            PlaneCorner[1] = Center - dis_x + dis_z;
            PlaneCorner[2] = Center + dis_x + dis_z;
            PlaneCorner[3] = Center + dis_x - dis_z;
        }

        public bool Inside(Vector3 point)
        {
            return Utility.Math.PointInPolygon(PlaneCorner, point);
        }

        public bool IsIntersect(RectParam rect)
        {
            var dir = rect.Center - Center;
            var dir_y = dir.y - Size.y * 0.5f;
            if (Mathf.Abs(dir_y) > (Size.y + rect.Size.y) * 0.5f)
            {
                return false;
            }

            return Utility.Math.IsIntersectRectWithRect(rect.PlaneCorner, PlaneCorner);
        }
    }

    public struct SectorParam : IShapeParam
    {
        public Vector3 Center { get; private set; }

        public float Radius { get; private set; }

        public float Angle { get; private set; }

        public float Height { get; private set; }

        public Vector3 NHFoward { get; private set; }

        public SectorParam(List<float> shape_param, ISceneUnit unit)
        {
            Radius       = shape_param.Count > 0 ? shape_param[0] : 0;
            Angle        = shape_param.Count > 1 ? shape_param[1] : 0;
            Height       = shape_param.Count > 2 ? shape_param[2] : 0;

            Center = Utility.Unit.GetPosition(unit);

            var dir = Utility.Unit.GetDirection(unit);
            dir.y = 0;
            NHFoward = dir.normalized;
        }

        public bool Inside(Vector3 point)
        {
            var dir = point - Center;
            dir.y = 0;

            if (dir.magnitude > Radius)
            {
                return false;
            }

            float angle = Vector3.Angle(dir, NHFoward);
            if (angle > 180)
            {
                angle = 360 - angle;
            }
            if (angle > Angle * 0.5f)
            {
                return false;
            }

            return true;
        }

        public bool IsIntersect(RectParam rect)
        {
            var dir = rect.Center - Center;
            var dir_y = dir.y - Height * 0.5f;
            if (Mathf.Abs(dir_y) > (Height + rect.Size.y) * 0.5f)
            {
                return false;
            }
            // Debug.LogError($"rect.Center:{rect.Center} + Center:{Center}");
            return Utility.Math.IsIntersectRectWithSector(rect.PlaneCorner, Center, NHFoward, Radius, Angle);
        }
    }

    public struct SphereParam : IShapeParam
    {
        public Vector3 Center { get; private set; }
        public float Radius { get; private set; }
        public SphereParam(List<float> shape_param, ISceneUnit unit)
        {
            Radius = shape_param.Count > 0 ? shape_param[0] : 0;
            Center = Utility.Unit.GetPosition(unit);
        }

        public bool Inside(Vector3 point)
        {
            var dir = point - Center;
            if (dir.magnitude > Radius)
            {
                return false;
            }
            return true;
        }

        ////不准确，算的是圆柱体
        public bool IsIntersect(RectParam rect)
        {
            var dir = rect.Center + new Vector3(0, rect.Size.y * 0.5f, 0) - Center;
            if (Mathf.Abs(dir.y) > (Radius + rect.Size.y) * 0.5f)
            {
                return false;
            }
            return Utility.Math.IsIntersectRectWithCircle(rect.PlaneCorner, Center, Radius);
        }
    }
}
