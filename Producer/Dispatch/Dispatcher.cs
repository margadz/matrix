﻿using Common.UOW;
using DataGenerator.Container;
using Producer.Matrix;
using Producer.MQ;
using Producer.Tools;

namespace Producer.Dispatch
{
    public class Dispatcher
    {
        private MatrixContainer container;
        private UOWGenerator uowGen;
        private MQClient MQClient;

        public Dispatcher(MatrixContainer container)
        {
            this.container = container;
            uowGen = new UOWGenerator(container);
        }

        public void Run()
        {
            using (MQClient = new MQClient())
            {
                UnitOfWork uow = uowGen.GenerateUOWFirstCalc();
                while(uow != null)
                {
                    MQClient.Call(uow);
                    uow = uowGen.GenerateUOWFirstCalc();
                }
            }
        }
    }
}
