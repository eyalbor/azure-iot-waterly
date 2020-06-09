import React from 'react'
import { connect } from 'react-redux'
import EventsTable from './EventTable'

class EventsList extends React.Component {

    renderList(){
        if(this.props.isSignedIn) {
            console.log(this.props.currentUserId)
            //this.props.fetchDevices(this.props.currentUserId);
            return (
               <EventsTable deviceId={this.props.deviceId}/>
            );
        } else {
            return <h3>Please sign in</h3>
        }
       
    }

    render(){
        return (
            <div className="ui container">
                {/* <h2>Device <u>{this.props.deviceId}</u> Events</h2> */}
                {this.renderList()}
            </div>
        );
    }
};

const mapStateToProps = (state, ownProps) => {
    return {
        deviceId: ownProps.match.params.id,
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn
    }
}

export default connect(mapStateToProps, null)(EventsList);