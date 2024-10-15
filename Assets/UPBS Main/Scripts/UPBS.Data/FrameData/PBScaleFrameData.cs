using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UPBS.Utility;

namespace UPBS.Data
{
    public class PBScaleFrameData : PBFrameDataBase
    {
        public Vector3 WorldPosition { get; protected set; } = Vector3.zero;
        public Vector3 EulerRotation { get; protected set; } = Vector3.zero;
        public Vector3 LocalScale { get; protected set; } = Vector3.zero;

        public PBScaleFrameData() : base()
        {

        }

        public PBScaleFrameData(PBScaleFrameData other) : base(other)
        {
            WorldPosition = other.WorldPosition;
            EulerRotation = other.EulerRotation;
            LocalScale = other.LocalScale;
        }

        protected override bool ParseRowInternal(PBFrameParser parser, string[] row, int rowNumber)
        {
            bool allClear = base.ParseRowInternal(parser, row, rowNumber);
            if (parser.GetColumnValuesAsFloats("WorldPosition", row, rowNumber, out float[] vals, WorldPosition.HeaderAppends()))
            {
                WorldPosition = new Vector3(vals[0], vals[1], vals[2]);
            }

            else
            {
                allClear = false;
                Debug.LogWarning($"WorldPosition value in row {rowNumber} could not be parsed!");
            }

            if (parser.GetColumnValuesAsFloats("EulerRotation", row, rowNumber, out vals, EulerRotation.HeaderAppends()))
            {
                EulerRotation = new Vector3(vals[0], vals[1], vals[2]);
            }

            else
            {
                allClear = false;
                Debug.LogWarning($"EulerRotation value in row {rowNumber} could not be parsed!");
            }

            if (parser.GetColumnValuesAsFloats("LocalScale", row, rowNumber, out vals, LocalScale.HeaderAppends()))
            {
                LocalScale = new Vector3(vals[0], vals[1], vals[2]);
            }

            else
            {
                allClear = false;
                Debug.LogWarning($"LocalScale value in row {rowNumber} could not be parsed!");
            }

            return allClear;
        }

        public override string[] GetClassHeader()
        {
            return base.GetClassHeader().Concat
                (
                WorldPosition.Header("WorldPosition"),
                EulerRotation.Header("EulerRotation"),
                LocalScale.Header("LocalScale")
                );
;
        }

        public override string[] GetVariableNameDisplay()
        {
            return base.GetVariableNameDisplay().Concat
            (
                new string[]
                {
                    "FPS"
                }
            );
        }
        public override string[] GetVariableErrorValuesDisplay()
        {
            return base.GetVariableErrorValuesDisplay().Concat
            (
                new string[]
                {
                    float.NaN.ToString(),
                }
            );
        }

        public override string[] GetVariableValuesDisplay()
        {

            return base.GetVariableValuesDisplay().Concat
            (
                new string[]
                {
                    WorldPosition.ToString(),
                }
            );
        }

        public override string[] GetVariableNullValuesDisplay()
        {
            return new string[] { "0" };
        }
    }
}
