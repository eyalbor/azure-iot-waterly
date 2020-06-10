//https://devexpress.github.io/devextreme-reactive/react/chart/

import React from 'react'
import { connect } from 'react-redux'
import { Link } from 'react-router-dom'
import DeviceTable from './DeviceTable'

class DeviceList extends React.Component {

    renderAdmin(device){
        if(device.userId === this.props.currentUserId){
            return (
                <div className="right floated content">
                    <div className="ui button">
                        Sync
                    </div>
                    <Link to={`/events/${device.device_id}`} className="ui secondary button">
                        Events
                    </Link>
                    <Link to={`/devices/edit/${device.device_id}`} className="ui button primary">
                        Edit
                    </Link>
                    <button className="ui button negative">
                        Delete
                    </button>
                </div>
            );
        }
    }
      

    renderList(){
        if(this.props.isSignedIn) {
            console.log(this.props.currentUserId)
            //this.props.fetchDevices(this.props.currentUserId);
            return (
               <DeviceTable userId={this.props.currentUserId}/>
            );
        } else {
            return <h3>Please sign in</h3>
        }
       
    }

    renderCreate(){
        if(this.props.isSignedIn){
            return(
                <div style={{textAlign:'right'}}>
                    <Link to="/devices/new" className="ui button primary">
                        Create Device
                    </Link>
                </div>
            );
        }
    }

    render() {
        return (
            <div className="ui container">
                {this.renderList()}
                <br/>
                {this.renderCreate()}
            </div>
        );
    }
}

//geting list of stream availble inside the component
const mapStateToProps = (state) => {
    //Object.values gets all the object inside and make it as array
    return {
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn
    }
}

export default connect(mapStateToProps, null)(DeviceList);