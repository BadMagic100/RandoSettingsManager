# Testing RandoSettingsManager Integration

This article describes one possible procedure to verify your integration with RandoSettingsManager is working as
expected.

## Testing Sending and Receiving Settings

The easiest method of testing your settings proxy is to create and load profiles meeting various conditions. This
allows you to see how your settings were serialized on disk and hand-modify the profile to easily create desired
test cases. Here is a good procedure to do that:

1. Enable your connection and load any profile which does not include your connection. Check that your connection
   has disabled itself (i.e. that you handled `null` properly). Note that you only need to put yourself into a state
   where you won't modify the hash, not necessarily disable all of your settings.
2. Save a profile while your connection is enabled. Check that your settings are saved as expected. They'll be stored
   in `%saves%/Randomizer 4/Profiles/YourProfileName/YourConnectionName/YourConnectionName.json`.
3. Disable a connection and load a profile with your settings. Check that your connection gets enabled and that the
   settings were set appropriately.
4. Save a profile while your connection is disabled. Make sure that nothing is saved for your connection.

## Testing Versioning

Testing versioning is a bit trickier. If you are using a simple versioning policy, you probably shouldn't bother
testing it. If you're using any nontrivial versioning policy (especially if you're implementing part or all of it
youreself) you can use a similar procedure to above. However, instead of saving a "real" profile, save a temporary
profile, and before leaving the settings management page, save a copy of the created ZIP file somewhere outside of
the profiles directory. You can then hand-modify the serialized version to see if compatibility breaks (or doesn't
break) as expected according to the changes you made when you try to reload that profile by putting a copy of the file
back into the profiles directory.