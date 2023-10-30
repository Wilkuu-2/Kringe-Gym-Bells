using UnityEngine;

[System.Serializable]
public class ShiftValue<T>
{
    [SerializeField] private T _current, _previous;
    public  T current {get => _current;}
    public  T previous {get => _previous;}
    
    public ShiftValue(T init_value){
        _current = init_value; 
        _previous = init_value;
    }

    public void Set(T value) {
        _previous = current; 
        _current = value;
    }

    public static implicit operator T(ShiftValue<T> input) {return input.current;} 

}
