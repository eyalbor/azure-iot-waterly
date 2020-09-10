import React from 'react'
import { connect } from 'react-redux'
import EventsTable from './EventTable'
import ScatterEvents from './charts/ScatterEvents'
import { fetchEvents, quality } from '../../actions/index'
import SpeedometerPH from './charts/SpeedometerPH'
import SpeedometerPressure from './charts/SpeedometerPressure'
import SpeedometerSalinity from './charts/SpeedometerSalinity'

class EventsList extends React.Component {

    componentDidMount(){
        this.props.fetchEvents(this.props.deviceId)
        this.props.quality(this.props.deviceId);
    }

    render(){
        if(this.props.isSignedIn)
        {
            return (
                <div className="ui container">
                    <h3>Device <u>{this.props.name}</u> Events</h3>
                    <EventsTable data={this.props.events} deviceId={this.props.deviceId}/>
                    <br/>
                    <div className="ui relaxed centered grid container">
                        <div class="three column centered row">
                            <div className="column">
                            {this.props.device_quality.ph!=null?<SpeedometerPH avg={this.props.device_quality.ph.toFixed(1)}/>:null}    
                            </div>
                            <div className="column">
                                {this.props.device_quality.pressure!=null?<SpeedometerPressure avg={this.props.device_quality.pressure.toFixed(1)}/>:null}
                            </div>
                            <div className="column">
                                {this.props.device_quality.pressure!=null?<SpeedometerSalinity avg={this.props.device_quality.salinity.toFixed(1)}/>:null}
                            </div>
                        </div>
                    </div>
                    <br/>
                    <ScatterEvents data={this.props.events}/>
                    <br/>
                </div>
            );
        }
        else{
            return <h3>Please sign in</h3>
        }
    }
};

const mapStateToProps = (state, ownProps) => {
    return {
        deviceId: ownProps.match.params.device_id,
        name:ownProps.match.params.name,
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn,
        events: Object.values(state.events),
        device_quality: state.quality
    }
}

export default connect(mapStateToProps, {fetchEvents, quality})(EventsList);