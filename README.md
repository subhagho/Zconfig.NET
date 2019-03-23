# ZConfig.NET

[![Build Status](https://travis-ci.org/subhagho/Zconfig.NET.svg?branch=master)](https://travis-ci.org/subhagho/Zconfig.NET)

DotNet port of the ZConfig Configuration Library.

# Freatures
- XML/JSON configuration definitions.
- Read from local/remote locations.
- Nested Configuration files (included)
- Auto-wiring of Objects
- Auto-invoke methods during auto-wiring

# Getting Started

## Configuration Sections
Creating a sample XML configuration file.

### Header
```xml
<header ID="[Unique ID]" group="[Application Group]" application="[Application Name]" name="[Config Name]" version="[0.2]">
    <description>[Description - optional]</description>
    <createdBy user="[username]" timestamp="[timestamp]" />
    <updatedBy user="[username]" timestamp="[timestamp]" />
 </header>
```

| Attribute | Description |
| --------- | ----------- |
| ID | Unique ID of the conifuration definition |
| group | Application group name |
| application | Application name |
| name | Configuration name |
| version | Configuration Version (X.X) |

| Element | Description |
| --------- | ----------- |
| Description | Configuration description (optional) |
| createdBy | Created By User/Timestamp (timestamp = epoch millis) |
| updatedBy | Updated By User/Timestamp (timestamp = epoch millis) |

### Configuration Body

#### Element Types
__Value Element__:
`<name>[value]</name>`

__Properties__: Used for scoped variable replacements
```xml
<properties>
      <property_name>[value]</property_name>
      <property_name>[value]</property_name>
      <property_name>[value]</property_name>
</properties>
```

__Parameters__: Used for auto-wiring method parameters
```xml
<parameters>
        <parameter_name>[string value ${PROP_1}]</parameter_name>
        <parameter_name>[short value]</parameter_name>
        <parameter_name>[enum value]</parameter_name>
        <parameter_name>[double value]</parameter_name>
</parameters>
```

__Lists__:
*Value Lists*: List of primitive/string values
__Note:__ all list element names should be the same.
```xml
<list_name>
        <element_name>[value]</element_name>
        <element_name>[value]</element_name>
        <element_name>[value]</element_name>
        ...
</list_name>
```
*Element Lists*: List of nested configuration elements.
```xml
<list_name>
        <element_name>
          <nodeVersion>1</nodeVersion>
          <owner>
            <user>john.doe</user>
            <timestamp>12.31.2018 12:32:19</timestamp>
          </owner>
          <string_1>STRING_1</string_1>
          <string_2>STRING_2</string_2>
          ...
        </element_name>
        <element_name>
          <description>This is a test description</description>
          <owner>
            <user>john.doe</user>
            <timestamp>12.31.2018 12:32:19</timestamp>
          </owner>
          
        </element_name>
        ...
</list_name>
```

__Include__:
Embedding another configuration
```xml
<include path="[Path/URL to configuration]" configName="[configuration name]"
                     type="[File|HTTP|HTTPS]" version="[0.*]">
</include>
```
__Note:__ Minor Version can be set to __(*)__ to denote match any.

## Reading Configuration (C#)
Code snippets to read the configuration files.

__Reader__:
Configuration Reader - Create an instance of the configuration reader, types supported
- FileReader - Read from local file
- RemoteReader - Read from a URL (HTTP|HTTPS)
- FtpReader - Read from a FTP location

__Parser__:
Configuration Parser based on the file type. Currently only XML is supported.
- XmlConfigParser - Parser to read XML configurations

__Snippent__:

Reading Configuration:

```csharp
using (FileReader reader = new FileReader(cfile))
{
    reader.Open();
    XmlConfigParser parser = new XmlConfigParser();
    ConfigurationSettings settings = new ConfigurationSettings();
    
    settings.DownloadOptions = EDownloadOptions.LoadRemoteResourcesOnStartup;

    parser.Parse(cname, reader, Version.Parse(version), settings);

    configuration = parser.GetConfiguration();

    return configuration;
}
```

