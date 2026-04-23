using System;
using OpenTK;

namespace NDS.NitroSystem.G3D
{
    public class MTX44
    {
        private const int MatrixSize = 4;
        private const int MatrixElementCount = 16;
        private float[] _matrixArray = new float[MatrixElementCount];

        public float this[int x, int y]
        {
            get => _matrixArray[x + y * MatrixSize];
            set => _matrixArray[x + y * MatrixSize] = value;
        }

        public float this[int index]
        {
            get => _matrixArray[index];
            set => _matrixArray[index] = value;
        }

        public float[] Floats => _matrixArray;

        public MTX44()
        {
            LoadIdentity();
        }

        public void SetValues(float[] array)
        {
            _matrixArray = array;
        }

        public MTX44 Clone()
        {
            MTX44 newMatrix = new MTX44();
            for (int i = 0; i < MatrixElementCount; i++)
            {
                newMatrix._matrixArray[i] = _matrixArray[i];
            }
            return newMatrix;
        }

        public void LoadIdentity()
        {
            Zero();
            this[0, 0] = 1;
            this[1, 1] = 1;
            this[2, 2] = 1;
            this[3, 3] = 1;
        }

        public void Zero()
        {
            Array.Clear(_matrixArray, 0, _matrixArray.Length);
        }

        public void CopyValuesTo(MTX44 targetMatrix)
        {
            for (int i = 0; i < MatrixElementCount; i++)
            {
                targetMatrix._matrixArray[i] = this[i];
            }
        }

        public void Translate(float x, float y, float z)
        {
            MTX44 translationMatrix = new MTX44();
            translationMatrix.LoadIdentity();
            translationMatrix[12] = x;
            translationMatrix[13] = y;
            translationMatrix[14] = z;
            MultMatrix(translationMatrix).CopyValuesTo(this);
        }

        public void Rotate(float x, float y, float z)
        {
            MTX44 rotationMatrix = new MTX44();
            rotationMatrix.LoadIdentity();
            float cosX = (float)Math.Cos(x);
            float sinX = (float)Math.Sin(x);
            float cosY = (float)Math.Cos(y);
            float sinY = (float)Math.Sin(y);
            float cosZ = (float)Math.Cos(z);
            float sinZ = (float)Math.Sin(z);
            rotationMatrix[0] = cosY * cosZ;
            rotationMatrix[1] = cosY * sinZ;
            rotationMatrix[2] = -sinY;
            rotationMatrix[4] = cosZ * sinX * sinY - sinZ * cosX;
            rotationMatrix[5] = sinX * sinY + cosX * cosZ;
            rotationMatrix[6] = sinX * cosY;
            rotationMatrix[8] = cosX * cosZ * sinY + sinX * sinZ;
            rotationMatrix[9] = cosX * sinY * sinZ - sinX * cosZ;
            rotationMatrix[10] = cosX * cosY;
            MultMatrix(rotationMatrix).CopyValuesTo(this);
        }

        public void Scale(float x, float y, float z)
        {
            MTX44 scaleMatrix = new MTX44();
            scaleMatrix.LoadIdentity();
            scaleMatrix[0] = x;
            scaleMatrix[5] = y;
            scaleMatrix[10] = z;
            MultMatrix(scaleMatrix).CopyValuesTo(this);
        }

        public MTX44 MultMatrix(MTX44 matrixB)
        {
            MTX44 resultMatrix = new MTX44();

            for (int row = 0; row < MatrixSize; row++)
            {
                for (int col = 0; col < MatrixSize; col++)
                {
                    resultMatrix._matrixArray[(row << 2) + col] = 0;

                    for (int k = 0; k < MatrixSize; k++)
                    {
                        resultMatrix._matrixArray[(row << 2) + col] +=
                            _matrixArray[(k << 2) + col] * matrixB._matrixArray[(row << 2) + k];
                    }
                }
            }

            return resultMatrix;
        }

        public Vector3 MultVector(Vector3 vector)
        {
            float[] floatVector = new[] { vector.X, vector.Y, vector.Z };
            float[] result = MultVector(floatVector);
            return new Vector3(result[0], result[1], result[2]);
        }

        public float[] MultVector(float[] vector)
        {
            float[] result = new float[3];

            for (int i = 0; i < 3; i++)
            {
                float xPart = vector[0] * this[i];
                float yPart = vector[1] * this[4 + i];
                float zPart = vector[2] * this[8 + i];
                float translatePart = this[12 + i];

                result[i] = xPart + yPart + zPart + translatePart;
            }

            return result;
        }

        public static MTX44 mtx_Rotate(int pivot, int neg, float a, float b)
        {
            float[] matrixArray = new float[MatrixElementCount]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 1
            };

            float sign = 1;
            float paramA = a;
            float paramB = b;
            switch (neg)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 9:
                case 11:
                case 13:
                case 15:
                    sign = -1;
                    break;
            }
            switch (neg)
            {
                case 2:
                case 3:
                case 6:
                case 7:
                case 10:
                case 11:
                case 14:
                case 15:
                    paramB = -paramB;
                    break;
            }
            switch (neg)
            {
                case 4:
                case 5:
                case 6:
                case 7:
                case 12:
                case 13:
                case 14:
                case 15:
                    paramA = -paramA;
                    break;
            }
            switch (pivot)
            {
                case 0:
                    matrixArray[0] = sign;
                    matrixArray[5] = paramA;
                    matrixArray[6] = paramB;
                    matrixArray[9] = paramB;
                    matrixArray[10] = paramA;
                    break;
                case 1:
                    matrixArray[1] = sign;
                    matrixArray[4] = paramA;
                    matrixArray[6] = paramB;
                    matrixArray[8] = paramB;
                    matrixArray[10] = paramA;
                    break;
                case 2:
                    matrixArray[2] = sign;
                    matrixArray[4] = paramA;
                    matrixArray[5] = paramB;
                    matrixArray[8] = paramB;
                    matrixArray[9] = paramA;
                    break;
                case 3:
                    matrixArray[4] = sign;
                    matrixArray[1] = paramA;
                    matrixArray[2] = paramB;
                    matrixArray[9] = paramB;
                    matrixArray[10] = paramA;
                    break;
                case 4:
                    matrixArray[5] = sign;
                    matrixArray[0] = paramA;
                    matrixArray[2] = paramB;
                    matrixArray[8] = paramB;
                    matrixArray[10] = paramA;
                    break;
                case 5:
                    matrixArray[6] = sign;
                    matrixArray[0] = paramA;
                    matrixArray[1] = paramB;
                    matrixArray[8] = paramB;
                    matrixArray[9] = paramA;
                    break;
                case 6:
                    matrixArray[8] = sign;
                    matrixArray[1] = paramA;
                    matrixArray[2] = paramB;
                    matrixArray[5] = paramB;
                    matrixArray[6] = paramA;
                    break;
                case 7:
                    matrixArray[9] = sign;
                    matrixArray[0] = paramA;
                    matrixArray[2] = paramB;
                    matrixArray[4] = paramB;
                    matrixArray[6] = paramA;
                    break;
                case 8:
                    matrixArray[10] = sign;
                    matrixArray[0] = paramA;
                    matrixArray[1] = paramB;
                    matrixArray[4] = paramB;
                    matrixArray[5] = paramA;
                    break;
                case 9:
                    matrixArray[0] = -paramA;
                    break;
            }

            MTX44 resultMatrix = new MTX44();
            resultMatrix._matrixArray = matrixArray;
            return resultMatrix;
        }

        public static MTX44 operator +(MTX44 a, MTX44 b)
        {
            MTX44 result = a.Clone();
            for (int i = 0; i < MatrixElementCount; i++)
                result[i] += b[i];
            return result;
        }

        public static MTX44 operator -(MTX44 a, MTX44 b)
        {
            MTX44 result = a.Clone();
            for (int i = 0; i < MatrixElementCount; i++)
                result[i] -= b[i];
            return result;
        }

        public static MTX44 operator *(MTX44 a, float b)
        {
            MTX44 result = a.Clone();
            for (int i = 0; i < MatrixElementCount; i++)
                result[i] *= b;
            return result;
        }

        public static MTX44 operator *(float b, MTX44 a)
        {
            return a * b;
        }

        public static MTX44 operator /(float b, MTX44 a)
        {
            MTX44 result = a.Clone();
            for (int i = 0; i < MatrixElementCount; i++)
                result[i] = b / result[i];
            return result;
        }

        public static implicit operator float[](MTX44 matrix)
        {
            return matrix.Floats;
        }

        public static implicit operator MTX44(float[] array)
        {
            MTX44 matrix = new MTX44();
            matrix._matrixArray = array;
            return matrix;
        }
    }
}