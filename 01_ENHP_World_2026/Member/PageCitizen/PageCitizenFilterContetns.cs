using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PageCitizenFilterContetns : MonoBehaviour
{
    [SerializeField] private Button BtnFilter;
    [SerializeField] private Button BtnSelectAll;

    private Action OnClickFilter = null;
    private Action OnClickSelectAll = null;

    public Action SetOnClickFilter { set { OnClickFilter = value; } }
    public Action SetOnClickSelectAll { set { OnClickSelectAll = value; } }
    
    public SingleAssignmentDisposable FilterBtnDisposer = null;
    public SingleAssignmentDisposable SelectAllBtnDisposer = null;


    public void InitUI()
    {
        if (FilterBtnDisposer != null)
        {
            FilterBtnDisposer.Dispose();
        }
        FilterBtnDisposer = new SingleAssignmentDisposable();
        FilterBtnDisposer.Disposable = BtnFilter.BindToOnClick(_ =>
        {
            OnClickFilter?.Invoke();
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);

        if (SelectAllBtnDisposer != null)
        {
            SelectAllBtnDisposer.Dispose();
        }
        SelectAllBtnDisposer = new SingleAssignmentDisposable();
        SelectAllBtnDisposer.Disposable = BtnSelectAll.BindToOnClick(_ =>
        {
            OnClickSelectAll?.Invoke();
            return Observable.Timer(TimeSpan.FromSeconds(CDefines.BUTTONDELAYTIME)).AsUnitObservable();
        }).AddTo(this);
    }

    public void Dispose()
    {
        
    }
}
