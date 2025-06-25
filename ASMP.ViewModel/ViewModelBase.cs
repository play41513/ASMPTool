using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ASMP.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {   //abstract表示不能new  需要靠繼承的
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {   //[CallerMemberName] C#特有的屬性 先呼叫此函式 就會自動帶進屬性名稱
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        /*
         * protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
         *  把屬性更新的標準流程打包成了一個。

            <T>: 表示這是一個泛型 (Generic) 方法。T 可以是任何類型，如 string, int, bool, Color 等。這讓這個方法非常通用。
            ref T field: ref 關鍵字表示傳入的是參考 (reference)，而不是複製一份。這讓方法可以直接修改那個屬性背後的私有欄位 (backing field)。
            步驟 A (檢查)：如果新值和舊值一樣，就沒有必要去更新 UI，否則可能造成不必要的畫面重繪，甚至無限迴圈。
            步驟 B (賦值)：更新私有欄位的值。
            步驟 C (通知)：呼叫 OnPropertyChanged 來廣播變更通知。[CallerMemberName] 在這裡同樣生效。
            步驟 D (返回)：返回 bool 值讓呼叫者知道是否真的發生了變更。
         */
    }
}