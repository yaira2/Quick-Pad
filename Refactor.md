# Quick Pad Refactor

## Architecture

The application is split into four projects:

* QuickPad.UI - User interface layer - only contains XAML and code behind
* QuickPad.MVVM - Model layer - only contains ViewModels and Models
* QuickPad.Data - Data Access layer - only contains Persistance.
* QuickPad.MVC - Controller and View Definition layer - only contains View definitions and the ApplicationController.

### Data Flow

#### Writing Data

UI -> ViewModel -> Controller -> Writer -> Persistence

1. The user clicks Save or Save As button.
2. The button is bound to the SaveCommand or SaveAsCommand properties of the DocumentViewModel and invokes it as normal
3. The ApplicationController gets a method invocation from the SimpleCommand&lt;DocumentViewModel&gt;.  
   a. The ApplicationController validates that a StorageFile has been assigned to the ViewModel.
      * Show the Save File dialog and capture StorageFile to the ViewModel.
   b. Pass the StorageFile, Text and Encoding to the Persistence layer.
4. Persistence layer converts the Text to a byte-array with the Encoding and saves it via the StorageFile. ** This need work.  It isn't generalized for other storage mechanisms yet.

#### Reading Data

Persistence -> Reader -> Controller -> ViewModel -> UI

1. The user clicks Load button.
2. The button is bound to the LoadCommand property of the DocumentViewModel and invokes it as normal.
3. The ApplicationController gets a method invocate from the SimpleCommand&lt;DocumentViewModel&gt;.
   a. The ApplicationController will show the Open File dialog.  If the user successfully selects a file, 
        the ApplicationController will prepare a new DocumentViewModel for the file.
   b. The ApplicationController passess the StorageFile object from the dialog to the Persistence layer
        which will load the file as a byte array and then attempt to convert it to a string using the
        specified Encoding object.
   c. The ApplicationController sets the StorageFile and Text to the new DocumentViewModel and assigns it to the Document View.
4. The MainPage's ViewModel property is updated by the ApplicationController, initiating an assignment to the DataContext.
5. The UI updates to reflect the contents of the new ViewModel.

### Where to do things:

* React to a button to manipulate text
  * The Button should be assigned to an instance of SimpleCommand&lt;ViewModel&gt; that is 
    hosted on the DocumentViewModel or another ViewModel that specifically handles that use case.
  * The ViewModel may supply the method to manipulate the text if the there are no data dependencies.

```csharp
public class MenuViewModel : ViewModel
{
    public SimpleCommand<DocumentViewModel> ToggleBoldText { get; } = 
        new SimpleCommand<DocumentViewModel>();

    public MenuViewModel()
    {
        ToggleBoldText.Executor = ToggleBoldText;
    }

    private async Task ToggleBoldText(DocumentViewModel documentViewModel)
    {
        // Using the Document property of the DocumentViewModel passed in,
        // Manipulate the Document to set the text bold or normal as appropriate.
    }
}
```

