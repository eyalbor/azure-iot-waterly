import React from 'react';
import { connect } from 'react-redux';
import { createDevice } from '../../actions';
import DeviceForm from './DeviceForm';

//we want helper method so we will use class
class DeviceCreate extends React.Component {

    onSubmit = (formValues) => {
        //console.log(formValues)
        this.props.createDevice(formValues)
    }

    render() {
        return (
          <div className="ui container">
              <h3>Create a Device</h3>
              <DeviceForm onSubmit={ this.onSubmit } />
          </div>
        );
    }; 
}

export default connect(null, { createDevice })(DeviceCreate)