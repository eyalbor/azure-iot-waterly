import _ from 'lodash'
import React from 'react'
import {connect} from 'react-redux'
import { fetchDevice, editDevice } from '../../actions'
import DeviceForm from './DeviceForm'

class DeviceEdit extends React.Component {
    componentDidMount(){
        this.props.fetchDevice(this.props.match.params.id);
    }

    onSumbit = (formValues) => {
       this.props.editDevice(this.props.match.params.id, formValues)
    }

    render(){
        if(!this.props.device){
            return <div>Loading...</div>
        }
        return (
            <div className="ui container">
                <h3>Edit a Device</h3>
                <DeviceForm
                initialValues={_.pick(this.props.device,'name', 'address')}
                //beacause the values in field in StreamForm called the same as in stream:title and description
                onSubmit={this.onSumbit} />
            </div>
        );
    }
};

const mapStateToProps = (state, ownProps) => {
    //Object.values gets all the object inside and make it as array
    return {
        device: state.devices[ownProps.match.params.id],
        // currentUserId: state.auth.userId,
        // isSignedIn: state.auth.isSignedIn
    }
}

export default connect(mapStateToProps, { fetchDevice, editDevice })(DeviceEdit);