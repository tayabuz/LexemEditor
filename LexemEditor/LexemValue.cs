using System.ComponentModel;

namespace LexemEditor
{
    public class LexemValue : INotifyPropertyChanged
    {
        public string Language { get; set; }

        public string Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                OnPropertyChanged(_Value);
            }
        }

        private string _Value;

        public override bool Equals(object obj)
        {
            LexemValue lexemValue;
            lexemValue = obj as LexemValue;
            return lexemValue == null ? false : Value == lexemValue.Value && Language == lexemValue.Language;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode() + this.Language.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, e);
        }

    }
}
