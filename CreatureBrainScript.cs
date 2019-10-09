using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBrainScript
{
    public Neuron[] neurons;
    public float[] outputs;

    public virtual void ReceiveSignal(List<float>[] inputSignals) {
        for (int i = 0; i < this.neurons.Length; i++) {
            this.neurons[i].SendOutput(inputSignals[i]);
        }
    }

    public virtual void Action() {

    }

    public virtual void UnconsciousAction() {

    }
}
