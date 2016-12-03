﻿using Common.Exceptions;
using Common.Interfaces;
using Raven.Client;
using Raven.Client.Document;
using System;
using System.Collections.Generic;

namespace MatrixAccess.RavenDBTools
{
    public class RavenDBMatrixSerializer : IMatrixSerializer, IDisposable
    {
        private IDocumentStore store;
        private IIdentifiers ids;

        public RavenDBMatrixSerializer(IIdentifiers ids)
        {
            this.ids = ids;
            store = new DocumentStore { Url = "http://localhost:8080/", DefaultDatabase = "matrix" };
            store.Initialize();
        }

        public List<int[][]> LoadInputMatrix()
        {
            MatrixType matrix;
            using (IDocumentSession session = store.OpenSession())
            {
                matrix = session.Load<MatrixType>(ids.InputMatrix);
            }
            if (matrix.Data.Count != 3)
            {
                throw new NumberOfMatrixesException("DB does not contain three input matrixes");
            }
            return matrix.Data;
        }

        public List<int[][]> LoadIntermediateMatrix()
        {
            MatrixType matrix;
            using (IDocumentSession session = store.OpenSession())
            {
                matrix = session.Load<MatrixType>(ids.IntermediateMatrix);
            }
            if (matrix.Data.Count != 1)
            {
                throw new NumberOfMatrixesException("DB does not contain intermediate matrix");
            }
            return matrix.Data;
        }

        public List<int[][]> LoadOutputMatrix()
        {
            MatrixType matrix;
            using (IDocumentSession session = store.OpenSession())
            {
                matrix = session.Load<MatrixType>(ids.OutputMatrix);
            }
            if (matrix.Data.Count != 1)
            {
                throw new NumberOfMatrixesException("DB does not contain output matrix");
            }
            return matrix.Data;
        }

        public void SaveInputMatrix(List<int[][]> matrix)
        {
            if (matrix.Count != 3)
            {
                throw new NumberOfMatrixesException("Three input matrixes have to be provided");
            }
            MatrixType m = new MatrixType(matrix);
            using (IDocumentSession session = store.OpenSession())
            {
                session.Delete(ids.InputMatrix);
                session.SaveChanges();

                session.Store(m, ids.InputMatrix);
                session.SaveChanges();
            }
        }

        public void SaveIntermediateMatrix(List<int[][]> matrix)
        {
            if (matrix.Count != 1)
            {
                throw new NumberOfMatrixesException("Single intermediate matrix have to be provided");
            }
            MatrixType m = new MatrixType(matrix);
            using (IDocumentSession session = store.OpenSession())
            {
                session.Delete(ids.IntermediateMatrix);
                session.SaveChanges();

                session.Store(m, ids.IntermediateMatrix);
                session.SaveChanges();
            }
        }

        public void SaveOutputMatrix(List<int[][]> matrix)
        {
            if (matrix.Count != 1)
            {
                throw new NumberOfMatrixesException("Single output matrix have to be provided");
            }
            MatrixType m = new MatrixType(matrix);
            using (IDocumentSession session = store.OpenSession())
            {
                session.Delete(ids.OutputMatrix);
                session.SaveChanges();

                session.Store(m, ids.OutputMatrix);
                session.SaveChanges();
            }
        }

        public void Dispose()
        {
            store.Dispose();
        }

        public void DeleteInputMatrix()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                session.Delete(ids.InputMatrix);
                session.SaveChanges();
            }
        }

        public void DeleteIntermediateMatrix()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                session.Delete(ids.IntermediateMatrix);
                session.SaveChanges();
            }
        }

        public void DeleteOutputMatrix()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                session.Delete(ids.OutputMatrix);
                session.SaveChanges();
            }
        }

        public void DeleteAllData()
        {
            DeleteInputMatrix();
            DeleteIntermediateMatrix();
            DeleteOutputMatrix();
        }
    }
}